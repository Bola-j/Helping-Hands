namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    private async Task<int> EnsureNotDueIntentsAsync(
        IReadOnlyList<StaleCharityTarget> charities, DateOnly date,
        DateTimeOffset scheduled, CancellationToken cancellationToken)
    {
        var created = 0;
        foreach (var charity in charities)
        {
            var result = await stateService.EnsureDailyIntentAsync(
                charity.RecipientId, date, scheduled, cancellationToken);
            if (result.Created) created++;
        }
        return created;
    }
}
