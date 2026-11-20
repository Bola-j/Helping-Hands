using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    private Task<DailyReminderIntent?> FindIntentAsync(
        Guid charityId, DateOnly cairoDate, CancellationToken cancellationToken) =>
        database.DailyReminderIntents.SingleOrDefaultAsync(
            row => row.CharityId == charityId && row.CairoDate == cairoDate, cancellationToken);

    private async Task<EnsureIntentResult> SaveOrResolveIntentAsync(
        DailyReminderIntent intent, Guid charityId, DateOnly cairoDate, CancellationToken cancellationToken)
    {
        try
        {
            await database.SaveChangesAsync(cancellationToken);
            return new EnsureIntentResult(intent, true);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Npgsql.PostgresException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation })
        {
            return await ResolveConcurrentIntentAsync(intent, charityId, cairoDate, cancellationToken);
        }
    }

    private async Task<EnsureIntentResult> ResolveConcurrentIntentAsync(
        DailyReminderIntent intent, Guid charityId, DateOnly cairoDate, CancellationToken cancellationToken)
    {
        database.Entry(intent).State = EntityState.Detached;
        var concurrent = await database.DailyReminderIntents.SingleAsync(
            row => row.CharityId == charityId && row.CairoDate == cairoDate, cancellationToken);
        return new EnsureIntentResult(concurrent, false);
    }

}
