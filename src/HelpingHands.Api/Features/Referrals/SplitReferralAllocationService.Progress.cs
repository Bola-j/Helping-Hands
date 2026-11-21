using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Referrals;

public sealed partial class SplitReferralAllocationService
{
    private static async Task<string> RequestAllocationStatusAsync(HelpingHandsDbContext database,
        AssistanceRequest request, RequestNeed? need, decimal remaining, CancellationToken cancellationToken)
    {
        if (need is null || remaining != 0m) return remaining == 0m ? "Allocated" : "Open";
        var otherNeedsAllocated = await database.RequestNeeds.Where(item => item.RequestId == request.Id && item.Id != need.Id)
            .AllAsync(item => item.Status == "Allocated" || item.Status == "Rejected", cancellationToken);
        return otherNeedsAllocated ? "Allocated" : "Open";
    }
}
