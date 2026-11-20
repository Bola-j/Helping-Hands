using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features.Reminders;

public sealed record EnsureIntentResult(DailyReminderIntent Intent, bool Created);

public sealed record BeginAttemptResult(
    bool CanProceed,
    string Reason,
    DailyReminderIntent Intent,
    ReminderDeliveryAttempt? Attempt);

public sealed partial class DailyReminderStateService(HelpingHandsDbContext database, TimeProvider timeProvider)
{
}
