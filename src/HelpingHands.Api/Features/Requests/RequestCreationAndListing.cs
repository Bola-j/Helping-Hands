using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> CreateRequest(RequestInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!input.AuthorityConfirmed || !input.SharingAgreed || Blank(input.Description, 4000)) return Bad("Description, authority and sharing confirmations are required.");
        var userId = Actor(user);
        if (!await database.Beneficiaries.AnyAsync(beneficiary => beneficiary.Id == input.BeneficiaryId && (Staff(user) || beneficiary.CreatedByUserId == userId || beneficiary.LinkedUserId == userId), cancellationToken)) return Results.NotFound();
        var request = new AssistanceRequest { BeneficiaryId = input.BeneficiaryId, SubmittedByUserId = Actor(user), Description = input.Description.Trim(), AuthorityConfirmed = true, SharingAgreed = true, Reference = await database.NextReferenceAsync("REQ", cancellationToken) };
        database.Requests.Add(request);
        database.DecisionHistory.Add(new DecisionHistory { RequestId = request.Id, ActorUserId = Actor(user), Action = "Submitted" });
        await database.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/v1/requests/{request.Id}", RequestView(request));
    }

    private static IQueryable<AssistanceRequest> VisibleRequests(HelpingHandsDbContext database, ClaimsPrincipal user)
    {
        var query = database.Requests.AsNoTracking();
        if (Staff(user)) return query;
        var userId = Actor(user);
        return query.Where(request => request.SubmittedByUserId == userId || database.Beneficiaries.Any(beneficiary => beneficiary.Id == request.BeneficiaryId && beneficiary.LinkedUserId == userId));
    }

    private static async Task<IResult> ListRequests(string? reference, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var query = VisibleRequests(database, user);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(request => request.Reference == reference);
        return Results.Ok((await query.OrderBy(request => request.Id).Take(100).ToListAsync(cancellationToken)).Select(RequestView));
    }
}
