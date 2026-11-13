using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> GetRequest(Guid id, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var request = await VisibleRequests(database, user).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return request is null ? Results.NotFound() : Results.Ok(RequestView(request));
    }

    private static async Task<IResult> UpdateRequest(Guid id, UpdateRequestInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!Beneficiary(user)) return Results.NotFound();
        if (Blank(input.Description, 4000)) return Bad("Description is required.");
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var request = await LockRequest(database, id, cancellationToken);
        if (request is null || request.SubmittedByUserId != Actor(user)) return Results.NotFound();
        if (request.IsMultiNeed) return Conflict("Multi-need clarification is not available yet.");
        if (request.Version != input.Version || request.Status != "Clarification") return Conflict("Request is not awaiting clarification or has changed.");
        request.Description = input.Description.Trim();
        request.Status = "Submitted";
        request.Version = Guid.NewGuid();
        database.DecisionHistory.Add(new DecisionHistory { RequestId = id, ActorUserId = Actor(user), Action = "ClarificationSubmitted" });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Ok(RequestView(request));
    }

    private static async Task<IResult> UpdateOutcome(Guid id, UpdateOutcomeInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (input.ReportedQuantity <= 0 || !Precision(input.ReportedQuantity) || Blank(input.Description, 4000)) return Bad("A positive reported quantity and description are required.");
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var outcome = await database.Outcomes.FromSqlInterpolated($"SELECT * FROM \"Outcome\" WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (outcome is null) return Results.NotFound();
        var referral = await database.Referrals.AsNoTracking().SingleAsync(item => item.Id == outcome.ReferralId, cancellationToken);
        if (!await Member(database, referral.RecipientId, Actor(user), cancellationToken)) return Results.NotFound();
        var request = await database.Requests.AsNoTracking().SingleAsync(item => item.Id == referral.RequestId, cancellationToken);
        if (outcome.Version != input.Version || outcome.Status is not ("Draft" or "Clarification")) return Conflict("Outcome changed or cannot be edited.");
        if (input.ReportedQuantity > referral.AssignedQuantity || (request.Unit == "scope" && input.ReportedQuantity != 1)) return Bad("Reported quantity exceeds the assignment or violates the scope unit.");
        outcome.ReportedQuantity = input.ReportedQuantity;
        outcome.Description = input.Description.Trim();
        outcome.Version = Guid.NewGuid();
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Ok(OutcomeView(outcome));
    }
}
