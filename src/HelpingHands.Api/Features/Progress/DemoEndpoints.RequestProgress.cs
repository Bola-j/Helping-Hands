using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Progress;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> GetRequestProgress(Guid id, ClaimsPrincipal user, HelpingHandsDbContext database,
        HelpingHands.Api.Features.Needs.NeedVersionService versions, CancellationToken cancellationToken)
    {
        var request = await VisibleProgressRequests(database, user).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (request is null) return Results.NotFound();
        var referrals = await VisibleProgressReferrals(database, request.Id, user).ToListAsync(cancellationToken);
        if (user.IsInRole("RecipientRepresentative") && referrals.Count == 0) return Results.NotFound();
        var history = await ProgressHistoryAsync(database, request, referrals, user, cancellationToken);
        var needs = await NeedProgressAsync(database, request, referrals, versions, cancellationToken);
        return Results.Ok(new RequestProgressView(request.Id, request.Reference, request.Status,
            user.IsInRole("RecipientRepresentative") ? null : request.Description, needs, history));
    }

    private static IQueryable<AssistanceRequest> VisibleProgressRequests(HelpingHandsDbContext database, ClaimsPrincipal user) =>
        Staff(user) || Beneficiary(user) ? VisibleRequests(database, user) :
            user.IsInRole("RecipientRepresentative") ? database.Requests.AsNoTracking() : database.Requests.Where(_ => false);

    private static IQueryable<Referral> VisibleProgressReferrals(HelpingHandsDbContext database, Guid requestId, ClaimsPrincipal user)
    {
        var query = database.Referrals.AsNoTracking().Where(item => item.RequestId == requestId);
        if (Staff(user) || Beneficiary(user)) return query;
        var userId = Actor(user);
        return query.Where(item => database.RecipientMembers.Any(member => member.RecipientId == item.RecipientId && member.UserId == userId && member.IsActive));
    }
}
