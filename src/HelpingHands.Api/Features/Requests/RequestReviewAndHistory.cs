using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<AssistanceRequest?> LockRequest(HelpingHandsDbContext database, Guid id, CancellationToken cancellationToken)
    {
        await database.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock_shared(726857101)", cancellationToken);
        return await database.Requests.FromSqlInterpolated($"SELECT * FROM \"AssistanceRequest\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
    }

    private static async Task<IResult> DecideRequest(Guid id, RequestDecisionInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!Coordinator(user)) return Results.Forbid();
        if (InvalidRequestDecision(input)) return Bad("Invalid decision or missing rejection reason.");
        if (input.Decision == "Accepted" && InvalidAcceptedRequest(input)) return Bad("An accepted request requires a valid category, quantity and unit.");
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var request = await LockRequest(database, id, cancellationToken);
        if (request is null) return Results.NotFound();
        if (request.IsMultiNeed) return Conflict("Multi-need review is not available yet.");
        if (request.Version != input.Version || request.Status is not ("Submitted" or "Clarification")) return Conflict("Request changed or cannot be reviewed.");
        ApplyRequestClassification(request, input);
        request.Status = input.Decision;
        request.Version = Guid.NewGuid();
        database.DecisionHistory.Add(new DecisionHistory { RequestId = id, ActorUserId = Actor(user), Action = input.Decision, Reason = input.Reason });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Ok(RequestView(request));
    }

    private static async Task<IResult> RequestHistory(Guid id, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!await VisibleRequests(database, user).AnyAsync(item => item.Id == id, cancellationToken)) return Results.NotFound();
        var query = database.DecisionHistory.AsNoTracking().Where(item => item.RequestId == id).OrderBy(item => item.OccurredAt);
        if (Staff(user)) return Results.Ok(await query.Select(item => new { item.Action, item.Reason, item.OccurredAt }).ToListAsync(cancellationToken));
        // Limit requester history to the status event, without staff-only reasons.
        return Results.Ok(await query.Select(item => new { item.Action, item.OccurredAt }).ToListAsync(cancellationToken));
    }
}
