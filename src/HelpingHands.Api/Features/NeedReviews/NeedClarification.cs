using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> ClarifyNeed(Guid requestId, Guid needId, NeedClarificationInput input, ClaimsPrincipal user, HelpingHandsDbContext db, CancellationToken ct)
    {
        if (!Beneficiary(user)) return Results.NotFound();
        if (Blank(input.Explanation, 2000)) return Bad("A clarification explanation is required.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var request = await LockRequest(db, requestId, ct);
        if (request is null || !request.IsMultiNeed || request.SubmittedByUserId != Actor(user)) return Results.NotFound();
        var needs = await db.RequestNeeds.Where(x => x.RequestId == requestId).ToListAsync(ct);
        var need = needs.SingleOrDefault(x => x.Id == needId);
        if (need is null) return Results.NotFound();
        if (request.Version != input.Version || need.Status != "Clarification") return Conflict("Request changed or the need is not awaiting clarification.");
        need.Status = "Submitted";
        db.NeedReviewHistory.Add(new NeedReviewHistory { NeedId = need.Id, ActorUserId = Actor(user), Action = "ClarificationSubmitted", Reason = input.Explanation.Trim() });
        RollupNeeds(request, needs);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Results.Ok(new { request.Id, request.Status, request.Version, NeedId = need.Id, NeedStatus = need.Status });
    }
}
