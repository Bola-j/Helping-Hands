using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CapacityUpdate_WhenHistoryInsertFails_RollsBackBalanceAndHistory()
    {
        var run = await PrepareCapacityAllocationRunAsync();
        using var representative = run.Representative;
        using var admin = run.Admin;
        using var coordinator = run.Coordinator;
        using var beneficiary = run.Beneficiary;
        using var allocationScope = run.AllocationScope;
        var connectionString = WorkflowConnectionString();
        var before = await ReadCapacityRollbackSnapshotAsync(run.Scenario.CharityServiceId);
        await using var trigger = await PostgresFailingInsertTrigger.InstallAsync(connectionString, "CapacityUpdate");
        using var response = await PutCapacityAsync(representative, run.Scenario);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(before, await ReadCapacityRollbackSnapshotAsync(run.Scenario.CharityServiceId));
    }

    private async Task<CapacityRollbackSnapshot> ReadCapacityRollbackSnapshotAsync(Guid charityServiceId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var capacity = await database.DailyCapacities.SingleAsync(item => item.CharityServiceId == charityServiceId);
        var updateCount = await database.CapacityUpdates.CountAsync(item => item.CapacityId == capacity.CapacityId);
        return new(capacity.Available, updateCount);
    }

    private string WorkflowConnectionString()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>().Database.GetConnectionString()!;
    }
}

internal sealed record CapacityRollbackSnapshot(decimal Available, int UpdateCount);
