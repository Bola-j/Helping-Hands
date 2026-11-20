using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    public async Task<(DailyReminderIntent Intent, ReminderDeliveryAttempt Attempt)> CompleteDeliveryAttemptAsync(
        Guid attemptId, bool succeeded, string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var (intent, attempt) = await LockAttemptStateAsync(attemptId, cancellationToken);
        if (attempt.Status != "InProgress")
        {
            await transaction.CommitAsync(cancellationToken);
            return (intent, attempt);
        }
        ApplyOutcome(intent, attempt, succeeded, failureReason, timeProvider.GetUtcNow());
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return (intent, attempt);
    }

    private static void EnsureAttemptParent(DailyReminderIntent intent, ReminderDeliveryAttempt attempt)
    {
        if (attempt.IntentId != intent.IntentId)
            throw new InvalidOperationException("Reminder attempt changed its parent intent while being completed.");
    }

    private async Task<(DailyReminderIntent Intent, ReminderDeliveryAttempt Attempt)> LockAttemptStateAsync(
        Guid attemptId, CancellationToken cancellationToken)
    {
        var intentId = await FindAttemptIntentIdAsync(attemptId, cancellationToken)
            ?? throw new InvalidOperationException($"Reminder delivery attempt '{attemptId}' does not exist.");
        var intent = await LockIntentAsync(intentId, cancellationToken);
        var attempt = await LockAttemptAsync(attemptId, cancellationToken);
        EnsureAttemptParent(intent, attempt);
        return (intent, attempt);
    }
}
