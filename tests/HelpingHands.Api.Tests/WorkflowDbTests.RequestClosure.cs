using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task MultiNeedClosureRequiresEveryAcceptedNeedAndRecordsOneCoordinator()
    {
        var scenario = await CreateMultiNeedClosureScenarioAsync();
        using var outsider = await fixture.LoginAsync("Unrelated");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var admin = await fixture.LoginAsync("Administrator");
        var path = $"/api/v1/requests/{scenario.RequestId}/closure";
        Assert.Equal(HttpStatusCode.Forbidden, (await WorkflowDbFixture.PostAsync(outsider, path, new { Version = scenario.Version })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await WorkflowDbFixture.PostAsync(coordinator, path, new { Version = scenario.Version })).StatusCode);
        await SeedAcceptedOutcomeAsync(scenario.ReferralIds[0]);
        Assert.Equal(HttpStatusCode.Conflict, (await WorkflowDbFixture.PostAsync(coordinator, path, new { Version = scenario.Version })).StatusCode);
        await AssertRequestNotClosedAsync(scenario.RequestId, scenario.Version);
        await SeedAcceptedOutcomeAsync(scenario.ReferralIds[1]);
        var winner = await AssertConcurrentClosureAsync(path, scenario.Version, coordinator, admin);
        await AssertClosedRequestAuditAsync(scenario.RequestId, scenario.Version, scenario.NeedIds, winner, coordinator);
        await AssertClosedAllocationRejectedAsync(scenario);
    }
}
