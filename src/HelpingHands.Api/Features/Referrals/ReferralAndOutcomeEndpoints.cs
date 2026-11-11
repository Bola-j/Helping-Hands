using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> GetReferral(Guid id, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var referral = await VisibleReferrals(database, user).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return referral is null ? Results.NotFound() : Results.Ok(ReferralView(referral));
    }

    private static async Task<IResult> ReferralHistory(Guid id, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!await VisibleReferrals(database, user).AnyAsync(item => item.Id == id, cancellationToken)) return Results.NotFound();
        return Results.Ok(await database.DecisionHistory.AsNoTracking().Where(item => item.ReferralId == id).OrderBy(item => item.OccurredAt).Select(item => new { item.Action, item.OccurredAt }).ToListAsync(cancellationToken));
    }

    private static async Task<IResult> CreateOutcome(Guid id, OutcomeInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (input.ReportedQuantity <= 0 || !Precision(input.ReportedQuantity) || Blank(input.Description, 4000)) return Bad("A positive reported quantity and description are required.");
        var referral = await database.Referrals.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (referral is null || !await Member(database, referral.RecipientId, Actor(user), cancellationToken)) return Results.NotFound();
        var request = await database.Requests.AsNoTracking().SingleAsync(item => item.Id == referral.RequestId, cancellationToken);
        if (input.ReportedQuantity > referral.AssignedQuantity || (request.Unit == "scope" && input.ReportedQuantity != 1)) return Bad("Reported quantity exceeds the assignment or violates the scope unit.");
        if (referral.Status is not ("Assigned" or "PartiallyFulfilled")) return Conflict("Referral is not open.");
        var outcome = new Outcome { ReferralId = id, ReportedByUserId = Actor(user), ReportedQuantity = input.ReportedQuantity, Description = input.Description.Trim(), Reference = await database.NextReferenceAsync("OUT", cancellationToken) };
        database.Outcomes.Add(outcome);
        await database.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/v1/outcomes/{outcome.Id}", OutcomeView(outcome));
    }
}
