using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    private async Task<bool> HasRecentAmbiguousFailureAsync(
        DailyReminderIntent intent, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (intent.Status != "Failed" || !intent.LastAttemptAt.HasValue) return false;
        var last = await LatestAttemptAsync(intent.IntentId, cancellationToken);
        return IsRecentAmbiguousFailure(last, now);
    }

    private Task<ReminderDeliveryAttempt?> LatestAttemptAsync(Guid intentId, CancellationToken cancellationToken) =>
        database.ReminderDeliveryAttempts.AsNoTracking()
            .Where(attempt => attempt.IntentId == intentId)
            .OrderByDescending(attempt => attempt.AttemptNumber)
            .FirstOrDefaultAsync(cancellationToken);

    private static bool IsRecentAmbiguousFailure(ReminderDeliveryAttempt? attempt, DateTimeOffset now) =>
        attempt is not null && attempt.Status == "Failed" && attempt.FailureReason is not null
        && attempt.FailureReason.StartsWith("Ambiguous:", StringComparison.Ordinal)
        && attempt.CompletedAt.HasValue && now - attempt.CompletedAt.Value < TimeSpan.FromHours(1);
}
