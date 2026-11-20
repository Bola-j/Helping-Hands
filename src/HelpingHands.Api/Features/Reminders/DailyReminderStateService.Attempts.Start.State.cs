using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    private static bool HasActiveLease(IEnumerable<ReminderDeliveryAttempt> attempts, DateTimeOffset now) =>
        attempts.Any(attempt => attempt.StartedAt > now - AttemptLease);

    private static void ExpireAttempts(IEnumerable<ReminderDeliveryAttempt> attempts, DateTimeOffset now)
    {
        foreach (var attempt in attempts)
        {
            attempt.Status = "Failed";
            attempt.FailureReason = "Interrupted before delivery completion (process restart/recovery).";
            attempt.CompletedAt = now;
        }
    }

}
