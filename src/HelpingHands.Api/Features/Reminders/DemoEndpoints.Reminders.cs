using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Reminders;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static void MapReminderRoutes(this RouteGroupBuilder api)
    {
        api.MapGet("/recipients/{id:guid}/reminders", GetCharityReminders).RequireAuthorization();
        api.MapGet("/reminders", ListDailyReminders).RequireAuthorization();
    }

    private static async Task<IResult> GetCharityReminders(
        Guid id,
        DateOnly? cairoDate,
        ClaimsPrincipal user,
        HelpingHandsDbContext database,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var userId = Actor(user);
        if (!Staff(user) && !await Member(database, id, userId, cancellationToken))
            return Results.NotFound();

        var charityExists = await database.Charities.AsNoTracking()
            .AnyAsync(item => item.RecipientId == id, cancellationToken);
        if (!charityExists)
            return Results.NotFound();

        var targetDate = cairoDate ?? DailyReminderStateService.GetCairoDate(timeProvider.GetUtcNow());

        var intents = await database.DailyReminderIntents.AsNoTracking()
            .Where(x => x.CharityId == id && x.CairoDate == targetDate)
            .ToListAsync(cancellationToken);

        var intentIds = intents.Select(x => x.IntentId).ToList();
        var attempts = intentIds.Count == 0
            ? []
            : await database.ReminderDeliveryAttempts.AsNoTracking()
                .Where(a => intentIds.Contains(a.IntentId))
                .OrderBy(a => a.AttemptNumber)
                .ToListAsync(cancellationToken);

        return Results.Ok(new
        {
            RecipientId = id,
            CairoDate = targetDate,
            Intents = intents.Select(i => new
            {
                i.IntentId,
                i.CharityId,
                i.CairoDate,
                i.ScheduledForUtc,
                i.Status,
                i.AttemptCount,
                i.CreatedAt,
                i.LastAttemptAt,
                i.DeliveredAt,
                Attempts = attempts
                    .Where(a => a.IntentId == i.IntentId)
                    .Select(a => new
                    {
                        a.AttemptId,
                        a.AttemptNumber,
                        a.Status,
                        a.RecipientEmail,
                        a.FailureReason,
                        a.StartedAt,
                        a.CompletedAt
                    })
            })
        });
    }

    private static async Task<IResult> ListDailyReminders(
        DateOnly? cairoDate,
        ClaimsPrincipal user,
        HelpingHandsDbContext database,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!Staff(user))
            return Results.Forbid();

        var targetDate = cairoDate ?? DailyReminderStateService.GetCairoDate(timeProvider.GetUtcNow());

        var intents = await database.DailyReminderIntents.AsNoTracking()
            .Where(x => x.CairoDate == targetDate)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var intentIds = intents.Select(x => x.IntentId).ToList();
        var attempts = intentIds.Count == 0
            ? []
            : await database.ReminderDeliveryAttempts.AsNoTracking()
                .Where(a => intentIds.Contains(a.IntentId))
                .OrderBy(a => a.AttemptNumber)
                .ToListAsync(cancellationToken);

        return Results.Ok(new
        {
            CairoDate = targetDate,
            Intents = intents.Select(i => new
            {
                i.IntentId,
                i.CharityId,
                i.CairoDate,
                i.ScheduledForUtc,
                i.Status,
                i.AttemptCount,
                i.CreatedAt,
                i.LastAttemptAt,
                i.DeliveredAt,
                Attempts = attempts
                    .Where(a => a.IntentId == i.IntentId)
                    .Select(a => new
                    {
                        a.AttemptId,
                        a.AttemptNumber,
                        a.Status,
                        a.RecipientEmail,
                        a.FailureReason,
                        a.StartedAt,
                        a.CompletedAt
                    })
            })
        });
    }
}