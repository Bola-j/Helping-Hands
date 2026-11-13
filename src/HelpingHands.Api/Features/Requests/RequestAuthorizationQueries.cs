using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static IQueryable<Beneficiary> VisibleBeneficiaries(HelpingHandsDbContext database, ClaimsPrincipal user)
    {
        var query = database.Beneficiaries.AsNoTracking();
        if (Staff(user)) return query;
        if (!Beneficiary(user)) return query.Where(_ => false);
        var userId = Actor(user);
        return query.Where(item => item.CreatedByUserId == userId || item.LinkedUserId == userId || database.Requests.Any(request => request.BeneficiaryId == item.Id && request.SubmittedByUserId == userId));
    }

    private static IQueryable<AssistanceRequest> VisibleRequests(HelpingHandsDbContext database, ClaimsPrincipal user)
    {
        var query = database.Requests.AsNoTracking();
        if (Staff(user)) return query;
        if (!Beneficiary(user)) return query.Where(_ => false);
        var userId = Actor(user);
        return query.Where(item => item.SubmittedByUserId == userId || database.Beneficiaries.Any(beneficiary => beneficiary.Id == item.BeneficiaryId && beneficiary.LinkedUserId == userId));
    }
}
