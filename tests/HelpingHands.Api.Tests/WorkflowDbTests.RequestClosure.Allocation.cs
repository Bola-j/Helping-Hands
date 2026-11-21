using HelpingHands.Api.Features.Referrals;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task AssertClosedAllocationRejectedAsync(MultiNeedClosureScenario scenario)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<SplitReferralAllocationService>();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var referralsBefore = await database.Referrals.CountAsync(item => item.RequestId == scenario.RequestId);
        var allocationsBefore = await database.DecisionHistory.CountAsync(item => item.RequestId == scenario.RequestId && item.Action == "SplitAllocated");
        var result = await service.AllocateSplitAsync(new(scenario.RequestId, scenario.NeedIds[0], fixture.UserIds["Coordinator"],
            [new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1m, "Fictional retry after closure")]));
        Assert.False(result.Succeeded);
        Assert.Equal("Closed", result.Status);
        Assert.Equal("RequestClosed", result.ErrorCode);
        Assert.Equal("Completed", result.RequestStatus);
        Assert.Empty(result.Legs);
        Assert.Equal(referralsBefore, await database.Referrals.CountAsync(item => item.RequestId == scenario.RequestId));
        Assert.Equal(allocationsBefore, await database.DecisionHistory.CountAsync(item => item.RequestId == scenario.RequestId && item.Action == "SplitAllocated"));
    }
}
