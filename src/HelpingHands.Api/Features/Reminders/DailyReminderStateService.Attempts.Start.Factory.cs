using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    private static ReminderDeliveryAttempt NewAttempt(
        Guid intentId, int attemptNumber, string? recipientEmail, DateTimeOffset now) => new()
    {
        IntentId = intentId,
        AttemptNumber = attemptNumber,
        Status = "InProgress",
        RecipientEmail = string.IsNullOrWhiteSpace(recipientEmail) ? null : recipientEmail.Trim(),
        StartedAt = now
    };

    private static void AdvanceIntent(DailyReminderIntent intent, int attemptNumber, DateTimeOffset now)
    {
        intent.AttemptCount = attemptNumber;
        intent.LastAttemptAt = now;
        intent.Version = Guid.NewGuid();
    }
}
