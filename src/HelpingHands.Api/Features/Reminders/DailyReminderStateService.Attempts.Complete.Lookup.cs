using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    private Task<Guid?> FindAttemptIntentIdAsync(Guid attemptId, CancellationToken cancellationToken) =>
        database.ReminderDeliveryAttempts.AsNoTracking()
            .Where(attempt => attempt.AttemptId == attemptId)
            .Select(attempt => (Guid?)attempt.IntentId)
            .SingleOrDefaultAsync(cancellationToken);
}
