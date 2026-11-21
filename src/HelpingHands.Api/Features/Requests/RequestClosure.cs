using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> CloseRequest(Guid id, VersionInput input, ClaimsPrincipal user,
        HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!Coordinator(user)) return Results.Forbid();
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var request = await LockRequest(database, id, cancellationToken);
        if (request is null) return Results.NotFound();
        if (request.Version != input.Version || request.Status is not ("Open" or "Completed")) return Conflict("Request changed or cannot be closed.");
        if (!await AllRequiredNeedsFulfilled(database, request, cancellationToken)) return Conflict("Every required need must be fulfilled and accepted before closure.");
        if (await database.DecisionHistory.AnyAsync(item => item.RequestId == id && item.Action == "RequestClosed", cancellationToken)) return Conflict("Request is already closed.");
        var now = DateTimeOffset.UtcNow;
        request.Status = "Completed";
        request.Version = Guid.NewGuid();
        database.DecisionHistory.Add(new DecisionHistory { RequestId = id, ActorUserId = Actor(user), Action = "RequestClosed", OccurredAt = now });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<bool> AllRequiredNeedsFulfilled(HelpingHandsDbContext database,
        AssistanceRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsMultiNeed) return await LegacyNeedFulfilled(database, request, cancellationToken);
        var needs = await database.RequestNeeds.AsNoTracking().Where(item => item.RequestId == request.Id).ToListAsync(cancellationToken);
        if (needs.Count == 0 || needs.Any(item => item.Status != "Accepted")) return false;
        var accepted = await AcceptedQuantitiesByNeed(database, request.Id, cancellationToken);
        return needs.All(need => accepted.GetValueOrDefault(need.Id) >= need.Quantity);
    }
}
