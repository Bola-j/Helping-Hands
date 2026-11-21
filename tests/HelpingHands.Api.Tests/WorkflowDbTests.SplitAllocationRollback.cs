using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task SplitAllocation_WhenHistoryInsertFails_RollsBackCapacityReferralAndHistory()
    {
        var scenario = await PrepareAcceptedNeedAllocationScenarioAsync();
        var connectionString = WorkflowConnectionString();
        var before = await ReadSplitAllocationRollbackSnapshotAsync(scenario);
        await using var trigger = await PostgresFailingInsertTrigger.InstallAsync(connectionString, "DecisionHistory");
        await Assert.ThrowsAsync<DbUpdateException>(() => AllocateNeedAsync(scenario));
        Assert.Equal(before, await ReadSplitAllocationRollbackSnapshotAsync(scenario));
    }

    private async Task<SplitAllocationRollbackSnapshot> ReadSplitAllocationRollbackSnapshotAsync(
        AcceptedNeedAllocationScenario scenario)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var charityServiceId = await database.CharityServices
            .Where(item => item.CharityId == scenario.RecipientId && item.ServiceVersionId == scenario.ServiceVersionId)
            .Select(item => item.CharityServiceId).SingleAsync();
        var capacity = await database.DailyCapacities.SingleAsync(item => item.CharityServiceId == charityServiceId);
        var request = await database.Requests.SingleAsync(item => item.Id == scenario.RequestId);
        var need = await database.RequestNeeds.SingleAsync(item => item.Id == scenario.NeedId);
        var referralIds = await database.Referrals.Where(item => item.RequestId == scenario.RequestId)
            .Select(item => item.Id).ToListAsync();
        var historyCount = await database.DecisionHistory.CountAsync(item => item.RequestId == scenario.RequestId ||
            (item.ReferralId.HasValue && referralIds.Contains(item.ReferralId.Value)));
        var updates = await database.CapacityUpdates.CountAsync(item => item.CapacityId == capacity.CapacityId);
        var referrals = await database.Referrals.CountAsync(item => item.RequestId == scenario.RequestId);
        return new(request.Status, request.Version, need.Status, capacity.Available,
            updates, referrals, historyCount);
    }

}

internal sealed record SplitAllocationRollbackSnapshot(
    string RequestStatus, Guid RequestVersion, string NeedStatus,
    decimal Available, int CapacityUpdates, int Referrals, int DecisionHistory);
