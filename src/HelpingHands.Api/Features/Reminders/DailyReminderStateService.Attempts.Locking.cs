using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    private async Task<DailyReminderIntent> LockIntentAsync(Guid intentId, CancellationToken cancellationToken) =>
        await database.DailyReminderIntents
            .FromSqlInterpolated($"SELECT * FROM \"DailyReminderIntent\" WHERE \"IntentId\" = {intentId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Reminder intent '{intentId}' does not exist.");

    private Task<List<ReminderDeliveryAttempt>> LockInProgressAttemptsAsync(
        Guid intentId, CancellationToken cancellationToken) =>
        database.ReminderDeliveryAttempts
            .FromSqlInterpolated($"SELECT * FROM \"ReminderDeliveryAttempt\" WHERE \"IntentId\" = {intentId} AND \"Status\" = 'InProgress' ORDER BY \"AttemptNumber\" FOR UPDATE")
            .ToListAsync(cancellationToken);

    private Task<ReminderDeliveryAttempt> LockAttemptAsync(Guid attemptId, CancellationToken cancellationToken) =>
        database.ReminderDeliveryAttempts
            .FromSqlInterpolated($"SELECT * FROM \"ReminderDeliveryAttempt\" WHERE \"AttemptId\" = {attemptId} FOR UPDATE")
            .SingleAsync(cancellationToken);
}
