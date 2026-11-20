using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    private static void ApplyOutcome(
        DailyReminderIntent intent, ReminderDeliveryAttempt attempt,
        bool succeeded, string? failureReason, DateTimeOffset now)
    {
        if (succeeded && IsValidEmail(attempt.RecipientEmail))
        {
            MarkDelivered(intent, attempt, now);
            return;
        }
        MarkFailed(intent, attempt, succeeded ? "No valid stored recipient email address." : failureReason, now);
    }

    private static void MarkDelivered(
        DailyReminderIntent intent, ReminderDeliveryAttempt attempt, DateTimeOffset now)
    {
        attempt.Status = "Succeeded";
        attempt.FailureReason = null;
        intent.Status = "Delivered";
        intent.DeliveredAt = now;
        intent.Version = Guid.NewGuid();
        attempt.CompletedAt = now;
    }

    private static void MarkFailed(
        DailyReminderIntent intent, ReminderDeliveryAttempt attempt, string? reason, DateTimeOffset now)
    {
        var detail = string.IsNullOrWhiteSpace(reason) ? "Delivery failed." : reason.Trim();
        attempt.Status = "Failed";
        attempt.FailureReason = detail[..Math.Min(detail.Length, 2000)];
        attempt.CompletedAt = now;
        if (intent.Status == "Delivered") return;
        intent.Status = "Failed";
        intent.DeliveredAt = null;
        intent.Version = Guid.NewGuid();
    }
}
