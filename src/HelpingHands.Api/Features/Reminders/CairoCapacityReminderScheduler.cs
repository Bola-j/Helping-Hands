using System.Text;
using HelpingHands.Api.Data;
using HelpingHands.Api.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelpingHands.Api.Features.Reminders;

public sealed record ReminderRunSummary(
    DateOnly CairoDate,
    DateTimeOffset ScheduledForUtc,
    int StaleCharityCount,
    int IntentsCreated,
    int DeliveredCount,
    int FailedCount,
    int AmbiguousCount,
    int SkippedAlreadyDeliveredCount,
    int SkippedNotDueCount);

public sealed class CairoCapacityReminderRunner(
    HelpingHandsDbContext database,
    DailyReminderStateService stateService,
    ISmtpEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<CairoCapacityReminderRunner> logger)
{
    public static readonly TimeOnly DailyReminderLocalTime = new(8, 0);

    public async Task<ReminderRunSummary> RunDueRemindersAsync(
        bool forceRunNow = false,
        CancellationToken cancellationToken = default)
    {
        var nowUtc = timeProvider.GetUtcNow();
        var cairoDate = DailyReminderStateService.GetCairoDate(nowUtc);
        var scheduledForUtc = DailyReminderStateService.GetScheduledUtcForCairoDay(cairoDate, DailyReminderLocalTime);

        var staleCharities = await FindStaleCharitiesAsync(database, cairoDate, cancellationToken);

        if (!forceRunNow && nowUtc < scheduledForUtc)
        {
            var notDueCreated = 0;
            foreach (var charity in staleCharities)
            {
                var ensured = await stateService.EnsureDailyIntentAsync(
                    charity.RecipientId,
                    cairoDate,
                    scheduledForUtc,
                    cancellationToken);
                if (ensured.Created)
                    notDueCreated++;
            }

            return new ReminderRunSummary(
                CairoDate: cairoDate,
                ScheduledForUtc: scheduledForUtc,
                StaleCharityCount: staleCharities.Count,
                IntentsCreated: notDueCreated,
                DeliveredCount: 0,
                FailedCount: 0,
                AmbiguousCount: 0,
                SkippedAlreadyDeliveredCount: 0,
                SkippedNotDueCount: staleCharities.Count);
        }

        var intentsCreated = 0;
        var deliveredCount = 0;
        var failedCount = 0;
        var ambiguousCount = 0;
        var skippedAlreadyDeliveredCount = 0;

        foreach (var charity in staleCharities)
        {
            var ensured = await stateService.EnsureDailyIntentAsync(
                charity.RecipientId,
                cairoDate,
                scheduledForUtc,
                cancellationToken);
            if (ensured.Created)
                intentsCreated++;

            var intent = ensured.Intent;
            if (intent.Status == "Delivered")
            {
                skippedAlreadyDeliveredCount++;
                continue;
            }

            if (intent.Status == "Failed" && intent.LastAttemptAt.HasValue)
            {
                var lastAttempt = await database.ReminderDeliveryAttempts.AsNoTracking()
                    .Where(a => a.IntentId == intent.IntentId)
                    .OrderByDescending(a => a.AttemptNumber)
                    .FirstOrDefaultAsync(cancellationToken);

                if (lastAttempt is not null &&
                    string.Equals(lastAttempt.Status, "Failed", StringComparison.Ordinal) &&
                    lastAttempt.FailureReason is not null &&
                    lastAttempt.FailureReason.StartsWith("Ambiguous:", StringComparison.Ordinal) &&
                    lastAttempt.CompletedAt.HasValue &&
                    (nowUtc - lastAttempt.CompletedAt.Value) < TimeSpan.FromHours(1))
                {
                    ambiguousCount++;
                    continue;
                }
            }

            var recipientEmail = string.IsNullOrWhiteSpace(charity.ContactDetails)
                ? $"charity-{charity.RecipientId:N}@placeholder.helpinghands.local"
                : charity.ContactDetails.Trim();

            var attemptBegin = await stateService.BeginDeliveryAttemptAsync(
                intent.IntentId,
                recipientEmail,
                cancellationToken);

            if (!attemptBegin.CanProceed || attemptBegin.Attempt is null)
            {
                skippedAlreadyDeliveredCount++;
                continue;
            }

            var message = BuildReminderEmail(charity, cairoDate, recipientEmail);
            SmtpSendResult sendResult;

            try
            {
                sendResult = await emailSender.SendAsync(message, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await stateService.CompleteDeliveryAttemptAsync(
                    attemptBegin.Attempt.AttemptId,
                    succeeded: false,
                    failureReason: "Ambiguous: Delivery cancelled during send attempt.",
                    cancellationToken: CancellationToken.None);
                throw;
            }
            catch (Exception ex)
            {
                sendResult = SmtpSendResult.Ambiguous(
                    errorCode: "SEND_EXCEPTION",
                    failureReason: $"Ambiguous exception while sending reminder: {ex.Message}");
            }

            if (sendResult.Outcome == SmtpSendOutcome.AcceptedByRelay)
            {
                await stateService.CompleteDeliveryAttemptAsync(
                    attemptBegin.Attempt.AttemptId,
                    succeeded: true,
                    failureReason: null,
                    cancellationToken);
                deliveredCount++;
            }
            else if (sendResult.Outcome == SmtpSendOutcome.Ambiguous)
            {
                var reason = $"Ambiguous: {sendResult.FailureReason ?? sendResult.StatusSummary}";
                await stateService.CompleteDeliveryAttemptAsync(
                    attemptBegin.Attempt.AttemptId,
                    succeeded: false,
                    failureReason: reason,
                    cancellationToken);
                ambiguousCount++;
                logger.LogWarning(
                    "Ambiguous SMTP outcome for charity {CharityId} on {CairoDate}: {Reason}",
                    charity.RecipientId,
                    cairoDate,
                    reason);
            }
            else
            {
                var reason = sendResult.FailureReason ?? sendResult.StatusSummary;
                await stateService.CompleteDeliveryAttemptAsync(
                    attemptBegin.Attempt.AttemptId,
                    succeeded: false,
                    failureReason: reason,
                    cancellationToken);
                failedCount++;
                logger.LogWarning(
                    "Failed SMTP delivery for charity {CharityId} on {CairoDate}: {Reason}",
                    charity.RecipientId,
                    cairoDate,
                    reason);
            }
        }

        return new ReminderRunSummary(
            CairoDate: cairoDate,
            ScheduledForUtc: scheduledForUtc,
            StaleCharityCount: staleCharities.Count,
            IntentsCreated: intentsCreated,
            DeliveredCount: deliveredCount,
            FailedCount: failedCount,
            AmbiguousCount: ambiguousCount,
            SkippedAlreadyDeliveredCount: skippedAlreadyDeliveredCount,
            SkippedNotDueCount: 0);
    }

    internal static async Task<IReadOnlyList<StaleCharityTarget>> FindStaleCharitiesAsync(
        HelpingHandsDbContext database,
        DateOnly cairoDate,
        CancellationToken cancellationToken)
    {
        var enabledServices = await (
            from cs in database.CharityServices.AsNoTracking()
            join r in database.Recipients.AsNoTracking() on cs.CharityId equals r.Id
            join c in database.Charities.AsNoTracking() on cs.CharityId equals c.RecipientId
            join sv in database.ServiceVersions.AsNoTracking() on cs.ServiceVersionId equals sv.Id
            join stv in database.StandardVersions.AsNoTracking() on cs.StandardVersionId equals stv.Id
            join us in database.UnitStandards.AsNoTracking() on stv.UnitStandardId equals us.Id
            where cs.Enabled
                && r.RecipientType == "Charity"
                && r.ApprovalStatus == "Approved"
                && r.Active
            select new
            {
                r.Id,
                r.DisplayName,
                r.ContactDetails,
                cs.CharityServiceId,
                ServiceName = sv.Name,
                sv.VersionNumber,
                StandardVersionNumber = stv.VersionNumber,
                UnitName = us.Name
            }
        ).ToListAsync(cancellationToken);

        if (enabledServices.Count == 0)
            return [];

        var serviceIds = enabledServices.Select(s => s.CharityServiceId).ToList();

        var freshServiceIds = (await database.DailyCapacities.AsNoTracking()
            .Where(dc => serviceIds.Contains(dc.CharityServiceId) && dc.CairoDate == cairoDate)
            .Select(dc => dc.CharityServiceId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        return enabledServices
            .Where(s => !freshServiceIds.Contains(s.CharityServiceId))
            .GroupBy(s => new { s.Id, s.DisplayName, s.ContactDetails })
            .OrderBy(g => g.Key.Id)
            .Select(g => new StaleCharityTarget(
                RecipientId: g.Key.Id,
                DisplayName: g.Key.DisplayName,
                ContactDetails: g.Key.ContactDetails,
                StaleServices: g.Select(x => $"{x.ServiceName} v{x.VersionNumber} ({x.UnitName} std v{x.StandardVersionNumber})").ToList()))
            .ToList();
    }

    private static EmailMessage BuildReminderEmail(
        StaleCharityTarget charity,
        DateOnly cairoDate,
        string recipientEmail)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Hello {charity.DisplayName},");
        sb.AppendLine();
        sb.AppendLine($"This is your daily capacity reminder for {cairoDate:yyyy-MM-dd} (Cairo local time).");
        sb.AppendLine("The following enabled services currently have stale or missing capacity for today:");
        foreach (var serviceLabel in charity.StaleServices)
            sb.AppendLine($"- {serviceLabel}");
        sb.AppendLine();
        sb.AppendLine("Please update your daily capacity so your services remain eligible for matching.");

        return new EmailMessage(
            ToAddress: recipientEmail,
            Subject: $"Daily Capacity Reminder - {cairoDate:yyyy-MM-dd} (Cairo)",
            PlainTextBody: sb.ToString());
    }

    internal sealed record StaleCharityTarget(
        Guid RecipientId,
        string DisplayName,
        string? ContactDetails,
        IReadOnlyList<string> StaleServices);
}

public sealed class CairoCapacityReminderHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<CairoCapacityReminderHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<CairoCapacityReminderRunner>();
                await runner.RunDueRemindersAsync(forceRunNow: false, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while executing scheduled 08:00 Cairo capacity reminders.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}