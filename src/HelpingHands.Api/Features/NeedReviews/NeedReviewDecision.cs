using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> DecideNeed(Guid requestId, Guid needId, NeedDecisionInput input, ClaimsPrincipal user, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (!Coordinator(user)) return Results.Forbid();
        if (InvalidNeedDecision(input)) return Bad("Invalid need decision, classification or reason.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var request = await LockRequest(db, requestId, ct);
        if (request is null || !request.IsMultiNeed) return Results.NotFound();
        var needs = await db.RequestNeeds.Where(x => x.RequestId == requestId).ToListAsync(ct);
        var need = needs.SingleOrDefault(x => x.Id == needId);
        if (need is null) return Results.NotFound();
        if (request.Version != input.Version || need.Status != "Submitted" || request.Status is not ("Submitted" or "InReview" or "Clarification"))
            return Conflict("Request changed or the need cannot be reviewed.");
        RecordNeedDecision(db, need, input, Actor(user));
        RollupNeeds(request, needs);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Results.Ok(new { request.Id, request.Status, request.Version, NeedId = need.Id, NeedStatus = need.Status });
    }
}
