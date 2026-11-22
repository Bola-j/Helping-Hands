using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static IQueryable<Outcome> VisibleOutcomes(HelpingHandsDbContext database, ClaimsPrincipal user)
    {
        var query = database.Outcomes.AsNoTracking();
        if (Coordinator(user)) return query;
        var userId = Actor(user);
        if (!user.IsInRole("RecipientRepresentative")) return query.Where(_ => false);
        return query.Where(outcome => CurrentRepresentativeUserIds(database).Contains(userId) && database.Referrals.Any(referral => referral.Id == outcome.ReferralId && database.RecipientMembers.Any(membership => membership.RecipientId == referral.RecipientId && membership.UserId == userId && membership.IsActive)));
    }

    private static async Task<IResult> ListOutcomes(string? reference, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var query = VisibleOutcomes(database, user);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(item => item.Reference == reference);
        return Results.Ok((await query.OrderBy(item => item.Id).Take(100).ToListAsync(cancellationToken)).Select(OutcomeView));
    }

    private static async Task<IResult> GetOutcome(Guid id, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var outcome = await VisibleOutcomes(database, user).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return outcome is null ? Results.NotFound() : Results.Ok(OutcomeView(outcome));
    }
}
