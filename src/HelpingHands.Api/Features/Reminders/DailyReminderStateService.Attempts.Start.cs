using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    private static readonly TimeSpan AttemptLease = TimeSpan.FromMinutes(5);

    public async Task<BeginAttemptResult> BeginDeliveryAttemptAsync(
        Guid intentId, string? recipientEmail, CancellationToken cancellationToken = default)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var intent = await LockIntentAsync(intentId, cancellationToken);
        var rejected = AlreadyDelivered(intent);
        if (rejected is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return rejected;
        }
        var result = await StartAttemptAsync(intent, intentId, recipientEmail, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private async Task<BeginAttemptResult> StartAttemptAsync(
        DailyReminderIntent intent, Guid intentId, string? recipientEmail, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var inProgress = await LockInProgressAttemptsAsync(intentId, cancellationToken);
        if (HasActiveLease(inProgress, now)) return new(false, "AttemptInProgress", intent, null);
        ExpireAttempts(inProgress, now);
        var attempt = NewAttempt(intentId, intent.AttemptCount + 1, recipientEmail, now);
        AdvanceIntent(intent, attempt.AttemptNumber, now);
        database.ReminderDeliveryAttempts.Add(attempt);
        await database.SaveChangesAsync(cancellationToken);
        return new(true, "Started", intent, attempt);
    }

    private static BeginAttemptResult? AlreadyDelivered(DailyReminderIntent intent) =>
        intent.Status == "Delivered" ? new(false, "AlreadyDelivered", intent, null) : null;
}
