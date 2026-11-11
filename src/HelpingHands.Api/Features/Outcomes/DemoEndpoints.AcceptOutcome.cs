using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<bool> ApplyAcceptedOutcome(HelpingHandsDbContext database, AssistanceRequest request,
        Referral referral, Outcome outcome, CancellationToken cancellationToken)
    {
        var accepted = await database.Outcomes.Where(item => item.ReferralId == referral.Id && item.Status == "Accepted")
            .SumAsync(item => (decimal?)item.ReportedQuantity, cancellationToken) ?? 0;
        if (accepted + outcome.ReportedQuantity > referral.AssignedQuantity) return false;
        referral.Status = accepted + outcome.ReportedQuantity == referral.AssignedQuantity ? "Completed" : "PartiallyFulfilled";
        referral.Version = Guid.NewGuid();
        var total = await (from item in database.Outcomes
                           join assigned in database.Referrals on item.ReferralId equals assigned.Id
                           where assigned.RequestId == request.Id && item.Status == "Accepted"
                           select (decimal?)item.ReportedQuantity).SumAsync(cancellationToken) ?? 0;
        if (total + outcome.ReportedQuantity == request.RequiredQuantity) request.Status = "Completed";
        request.Version = Guid.NewGuid();
        return true;
    }
}
