using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> MapLegacyStandard(Guid id, LegacyStandardInput input, ClaimsPrincipal user,
        HelpingHandsDbContext database, TimeProvider clock, CancellationToken cancellationToken)
    {
        if (!Coordinator(user)) return Results.Forbid();
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var request = await LockRequest(database, id, cancellationToken);
        if (request is null) return Results.NotFound();
        if (request.IsMultiNeed || request.Status is not ("Accepted" or "Open") || request.Version != input.Version || request.LegacyStandardVersionId is not null)
            return Conflict("Request changed or cannot be mapped.");
        var now = clock.GetUtcNow();
        if (!await IsEligibleLegacyStandard(database, input.StandardVersionId, now, cancellationToken))
            return Conflict("The selected standard is not published and effective.");
        request.LegacyStandardVersionId = input.StandardVersionId;
        request.LegacyMappingActorId = Actor(user);
        request.LegacyMappedAt = now;
        request.Version = Guid.NewGuid();
        database.DecisionHistory.Add(new DecisionHistory { RequestId = id, ActorUserId = Actor(user), Action = "LegacyStandardMapped", OccurredAt = now });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Ok(RequestView(request));
    }
}
