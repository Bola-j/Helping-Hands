using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CapacityUpdateAndReferralAllocation_SerializeOnPostgresRows()
    {
        var run = await PrepareCapacityAllocationRunAsync();
        using var representative = run.Representative;
        using var admin = run.Admin;
        using var coordinator = run.Coordinator;
        using var beneficiary = run.Beneficiary;
        using var allocationScope = run.AllocationScope;
        var result = await RunWritersAfterProvenContentionAsync(run);
        using var response = result.CapacityResponse;
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.True(result.Allocation.Succeeded);
        Assert.Equal((1, 1m), await fixture.ReferralTotalsAsync(run.Scenario.RequestId));
        await AssertCapacityAndRequestStateAsync(run.Scenario.RequestId, run.Scenario.CharityServiceId);
    }

    private async Task AssertCapacityAndRequestStateAsync(Guid requestId, Guid charityServiceId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var request = await database.Requests.SingleAsync(item => item.Id == requestId);
        var capacity = await database.DailyCapacities.SingleAsync(item => item.CharityServiceId == charityServiceId);
        var updates = await database.CapacityUpdates.CountAsync(item => item.CapacityId == capacity.CapacityId);
        var referralId = await database.Referrals.Where(item => item.RequestId == requestId)
            .Select(item => item.Id).SingleAsync();
        var history = await database.DecisionHistory.Where(item => item.RequestId == requestId || item.ReferralId == referralId)
            .ToListAsync();
        Assert.Equal("Open", request.Status);
        Assert.True(capacity.Available >= 0m);
        Assert.Equal(2, updates);
        Assert.Contains(history, item => item.RequestId == requestId && item.ReferralId is null && item.Action == "SplitAllocated");
        Assert.Contains(history, item => item.RequestId is null && item.ReferralId == referralId && item.Action == "SplitAllocated");
    }
}
