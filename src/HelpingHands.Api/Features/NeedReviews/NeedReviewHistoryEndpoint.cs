using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> NeedHistory(Guid requestId, Guid needId, ClaimsPrincipal user, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (!await VisibleRequests(db, user).AnyAsync(x => x.Id == requestId && x.IsMultiNeed, ct)) return Results.NotFound();
        if (!await db.RequestNeeds.AnyAsync(x => x.Id == needId && x.RequestId == requestId, ct)) return Results.NotFound();
        var entries = db.NeedReviewHistory.AsNoTracking().Where(x => x.NeedId == needId).OrderBy(x => x.OccurredAt).ThenBy(x => x.Id);
        if (Staff(user)) return Results.Ok(await entries.Select(x => new { x.Action, x.ActorUserId, x.Reason, x.Category, x.ServiceSubtype, x.OccurredAt }).ToListAsync(ct));
        return Results.Ok(await entries.Select(x => new { x.Action, x.Reason, x.Category, x.ServiceSubtype, x.OccurredAt }).ToListAsync(ct));
    }
}
