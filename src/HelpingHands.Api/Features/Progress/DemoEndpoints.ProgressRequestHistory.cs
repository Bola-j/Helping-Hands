using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Progress;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<List<ProgressHistoryEvent>> RequestAndReferralHistoryAsync(HelpingHandsDbContext database,
        AssistanceRequest request, IReadOnlyList<Referral> referrals, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var referralIds = referrals.Select(item => item.Id).ToArray();
        var query = database.DecisionHistory.AsNoTracking();
        query = Staff(user) || Beneficiary(user)
            ? query.Where(item => item.RequestId == request.Id || referralIds.Contains(item.ReferralId ?? Guid.Empty))
            : query.Where(item => referralIds.Contains(item.ReferralId ?? Guid.Empty));
        var rows = await query.OrderBy(item => item.OccurredAt).ToListAsync(cancellationToken);
        return rows.Select(item => new ProgressHistoryEvent(item.ReferralId is null ? "Request" : "Referral",
            item.ReferralId ?? request.Id, item.Action, item.OccurredAt, Staff(user) ? item.ActorUserId : null,
            Staff(user) ? item.Reason : null)).ToList();
    }
}
