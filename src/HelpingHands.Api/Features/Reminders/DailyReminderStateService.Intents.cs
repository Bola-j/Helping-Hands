using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class DailyReminderStateService
{
    public async Task<EnsureIntentResult> EnsureDailyIntentAsync(
        Guid charityId, DateOnly cairoDate, DateTimeOffset? scheduledForUtc = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindIntentAsync(charityId, cairoDate, cancellationToken);
        if (existing is not null) return new EnsureIntentResult(existing, false);
        var intent = CreatePendingIntent(charityId, cairoDate, scheduledForUtc);
        database.DailyReminderIntents.Add(intent);
        return await SaveOrResolveIntentAsync(intent, charityId, cairoDate, cancellationToken);
    }

    private DailyReminderIntent CreatePendingIntent(
        Guid charityId, DateOnly cairoDate, DateTimeOffset? scheduledForUtc) => new()
    {
        CharityId = charityId,
        CairoDate = cairoDate,
        ScheduledForUtc = scheduledForUtc ?? GetScheduledUtcForCairoDay(cairoDate),
        Status = "Pending",
        CreatedAt = timeProvider.GetUtcNow()
    };
}
