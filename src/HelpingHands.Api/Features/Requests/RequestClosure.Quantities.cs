using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<Dictionary<Guid, decimal>> AcceptedQuantitiesByNeed(
        HelpingHandsDbContext database, Guid requestId, CancellationToken cancellationToken) =>
        await (from outcome in database.Outcomes
               join referral in database.Referrals on outcome.ReferralId equals referral.Id
               where referral.RequestId == requestId && referral.NeedId != null && outcome.Status == "Accepted"
               group outcome by referral.NeedId!.Value into needOutcomes
               select new { NeedId = needOutcomes.Key, Quantity = needOutcomes.Sum(item => item.ReportedQuantity) })
            .ToDictionaryAsync(item => item.NeedId, item => item.Quantity, cancellationToken);

    private static async Task<bool> LegacyNeedFulfilled(HelpingHandsDbContext database,
        AssistanceRequest request, CancellationToken cancellationToken)
    {
        if (request.RequiredQuantity is not > 0m) return false;
        var accepted = await (from outcome in database.Outcomes
                              join referral in database.Referrals on outcome.ReferralId equals referral.Id
                              where referral.RequestId == request.Id && referral.NeedId == null && outcome.Status == "Accepted"
                              select (decimal?)outcome.ReportedQuantity).SumAsync(cancellationToken) ?? 0m;
        return accepted >= request.RequiredQuantity.Value;
    }
}
