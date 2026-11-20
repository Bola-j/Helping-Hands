using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HelpingHands.Api.Features.Reminders;

public sealed record EnsureIntentResult(
    DailyReminderIntent Intent,
    bool Created);

public sealed record BeginAttemptResult(
    bool CanProceed,
    string Reason,
    DailyReminderIntent Intent,
    ReminderDeliveryAttempt? Attempt);

public sealed class DailyReminderStateService(HelpingHandsDbContext database, TimeProvider timeProvider)
{
    public static readonly TimeZoneInfo CairoTimeZone = ResolveCairoTimeZone();

    private static TimeZoneInfo ResolveCairoTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        }
    }

    public static DateOnly GetCairoDate(DateTimeOffset utcNow)
    {
        var cairoTime = TimeZoneInfo.ConvertTime(utcNow, CairoTimeZone);
        return DateOnly.FromDateTime(cairoTime.DateTime);
    }

    public static DateTimeOffset GetScheduledUtcForCairoDay(DateOnly cairoDate, TimeOnly? localTime = null)
    {
        var time = localTime ?? new TimeOnly(8, 0);
        var unspecified = cairoDate.ToDateTime(time, DateTimeKind.Unspecified);
        var offset = CairoTimeZone.GetUtcOffset(unspecified);
        return new DateTimeOffset(unspecified, offset).ToUniversalTime();
    }

    /// <summary>
    /// Ensures at most one reminder intent exists per (CharityId, CairoDate).
    /// Safe across process restarts and concurrent invocations without overwriting existing state.
    /// </summary>
    public async Task<EnsureIntentResult> EnsureDailyIntentAsync(
        Guid charityId,
        DateOnly cairoDate,
        DateTimeOffset? scheduledForUtc = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await database.DailyReminderIntents
            .SingleOrDefaultAsync(x => x.CharityId == charityId && x.CairoDate == cairoDate, cancellationToken);

        if (existing is not null)
            return new EnsureIntentResult(existing, Created: false);

        var now = timeProvider.GetUtcNow();
        var intent = new DailyReminderIntent
        {
            CharityId = charityId,
            CairoDate = cairoDate,
            ScheduledForUtc = scheduledForUtc ?? GetScheduledUtcForCairoDay(cairoDate),
            Status = "Pending",
            AttemptCount = 0,
            CreatedAt = now
        };

        database.DailyReminderIntents.Add(intent);

        try
        {
            await database.SaveChangesAsync(cancellationToken);
            return new EnsureIntentResult(intent, Created: true);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            database.Entry(intent).State = EntityState.Detached;
            var concurrent = await database.DailyReminderIntents
                .SingleAsync(x => x.CharityId == charityId && x.CairoDate == cairoDate, cancellationToken);
            return new EnsureIntentResult(concurrent, Created: false);
        }
    }

    /// <summary>
    /// Starts a new delivery attempt without claiming delivery.
    /// Refuses duplicate delivery once Delivered, and marks any orphaned InProgress attempt from a prior crash/restart as Failed before retrying.
    /// </summary>
    public async Task<BeginAttemptResult> BeginDeliveryAttemptAsync(
        Guid intentId,
        string? recipientEmail,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        var intent = await database.DailyReminderIntents
            .FromSqlInterpolated($"SELECT * FROM \"DailyReminderIntent\" WHERE \"IntentId\" = {intentId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (intent is null)
            throw new InvalidOperationException($"Reminder intent '{intentId}' does not exist.");

        if (intent.Status == "Delivered")
        {
            await transaction.CommitAsync(cancellationToken);
            return new BeginAttemptResult(
                CanProceed: false,
                Reason: "AlreadyDelivered",
                Intent: intent,
                Attempt: null);
        }

        var now = timeProvider.GetUtcNow();

        // Recover any orphaned InProgress attempts left behind by a process crash or restart
        var orphanedAttempts = await database.ReminderDeliveryAttempts
            .Where(a => a.IntentId == intentId && a.Status == "InProgress")
            .ToListAsync(cancellationToken);

        foreach (var orphan in orphanedAttempts)
        {
            orphan.Status = "Failed";
            orphan.FailureReason = "Interrupted before delivery completion (process restart/recovery).";
            orphan.CompletedAt = now;
        }

        var nextAttemptNumber = intent.AttemptCount + 1;
        intent.AttemptCount = nextAttemptNumber;
        intent.LastAttemptAt = now;
        intent.Version = Guid.NewGuid();

        var attempt = new ReminderDeliveryAttempt
        {
            IntentId = intentId,
            AttemptNumber = nextAttemptNumber,
            Status = "InProgress",
            RecipientEmail = string.IsNullOrWhiteSpace(recipientEmail) ? null : recipientEmail.Trim(),
            StartedAt = now
        };

        database.ReminderDeliveryAttempts.Add(attempt);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new BeginAttemptResult(
            CanProceed: true,
            Reason: "Started",
            Intent: intent,
            Attempt: attempt);
    }

    /// <summary>
    /// Records the final outcome of an attempt. Delivery is only claimed when succeeded is true.
    /// </summary>
    public async Task<(DailyReminderIntent Intent, ReminderDeliveryAttempt Attempt)> CompleteDeliveryAttemptAsync(
        Guid attemptId,
        bool succeeded,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        var attempt = await database.ReminderDeliveryAttempts
            .FromSqlInterpolated($"SELECT * FROM \"ReminderDeliveryAttempt\" WHERE \"AttemptId\" = {attemptId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (attempt is null)
            throw new InvalidOperationException($"Reminder delivery attempt '{attemptId}' does not exist.");

        var intent = await database.DailyReminderIntents
            .FromSqlInterpolated($"SELECT * FROM \"DailyReminderIntent\" WHERE \"IntentId\" = {attempt.IntentId} FOR UPDATE")
            .SingleAsync(cancellationToken);

        if (attempt.Status != "InProgress")
        {
            await transaction.CommitAsync(cancellationToken);
            return (intent, attempt);
        }

        var now = timeProvider.GetUtcNow();
        attempt.CompletedAt = now;

        if (succeeded)
        {
            attempt.Status = "Succeeded";
            attempt.FailureReason = null;

            intent.Status = "Delivered";
            intent.DeliveredAt = now;
            intent.Version = Guid.NewGuid();
        }
        else
        {
            attempt.Status = "Failed";
            attempt.FailureReason = string.IsNullOrWhiteSpace(failureReason)
                ? "Delivery failed."
                : failureReason.Trim()[..Math.Min(failureReason.Trim().Length, 2000)];

            if (intent.Status != "Delivered")
            {
                intent.Status = "Failed";
                intent.DeliveredAt = null;
                intent.Version = Guid.NewGuid();
            }
        }

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (intent, attempt);
    }
}