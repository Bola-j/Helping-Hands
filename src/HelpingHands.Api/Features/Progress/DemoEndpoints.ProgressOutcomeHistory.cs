using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Progress;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<List<ProgressHistoryEvent>> OutcomeReviewHistoryAsync(HelpingHandsDbContext database,
        IReadOnlyList<Referral> referrals, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var referralIds = referrals.Select(item => item.Id).ToArray();
        var outcomeIds = await database.Outcomes.AsNoTracking().Where(item => referralIds.Contains(item.ReferralId))
            .Select(item => item.Id).ToArrayAsync(cancellationToken);
        var rows = await database.OutcomeReviews.AsNoTracking().Where(item => outcomeIds.Contains(item.OutcomeId))
            .OrderBy(item => item.ReviewedAt).ToListAsync(cancellationToken);
        return rows.Select(item => new ProgressHistoryEvent("Outcome", item.OutcomeId, item.Decision, item.ReviewedAt,
            Staff(user) ? item.ActorUserId : null,
            Staff(user) || item.Decision == "Clarification" ? item.Reason : null)).ToList();
    }
}
