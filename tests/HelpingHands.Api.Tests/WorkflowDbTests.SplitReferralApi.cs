using System.Net;
using HelpingHands.Api.Features.Referrals;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task PerNeedSplitReferralApi_AllocatesForCoordinatorAndAdministratorOnly()
    {
        var scenario = await PrepareAcceptedNeedAllocationScenarioAsync();
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var admin = await fixture.LoginAsync("Administrator");
        using var outsider = await fixture.LoginAsync("Unrelated");
        var version = await SplitReferralApiHelpers.RequestVersionAsync(fixture, scenario.RequestId);
        var path = SplitReferralApiHelpers.Path(scenario.RequestId, scenario.NeedId);
        var legs = new[] { SplitReferralApiHelpers.Leg(scenario, 0.4m), SplitReferralApiHelpers.Leg(scenario, 0.6m) };

        using var denied = await WorkflowDbFixture.PostAsync(outsider, path, new { Version = version, Allocations = legs });
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var allocated = await WorkflowDbFixture.PostAsync(coordinator, path, new { Version = version, Allocations = legs });
        var body = await WorkflowDbFixture.JsonAsync(allocated, HttpStatusCode.Created);
        Assert.Equal(2, body.GetProperty("legs").GetArrayLength());
        var ids = body.GetProperty("legs").EnumerateArray().Select(leg =>
        {
            Assert.Equal(scenario.NeedId, leg.GetProperty("needId").GetGuid());
            Assert.False(string.IsNullOrWhiteSpace(leg.GetProperty("referralReference").GetString()));
            return leg.GetProperty("referralId").GetGuid();
        }).ToArray();
        await SplitReferralApiAssertions.ReferralsAsync(fixture, scenario, 2, 1m, ids);

        var secondScenario = await PrepareAcceptedNeedAllocationScenarioAsync();
        var secondVersion = await SplitReferralApiHelpers.RequestVersionAsync(fixture, secondScenario.RequestId);
        using var adminAllocated = await WorkflowDbFixture.PostAsync(admin,
            SplitReferralApiHelpers.Path(secondScenario.RequestId, secondScenario.NeedId),
            new { Version = secondVersion, Allocations = new[] { SplitReferralApiHelpers.Leg(secondScenario, 1m) } });
        Assert.Equal(HttpStatusCode.Created, adminAllocated.StatusCode);
    }

    [Fact]
    public async Task PerNeedSplitReferralApi_RejectsStaleAndInvalidAllocationsAtomically()
    {
        var scenario = await PrepareAcceptedNeedAllocationScenarioAsync();
        using var coordinator = await fixture.LoginAsync("Coordinator");
        var version = await SplitReferralApiHelpers.RequestVersionAsync(fixture, scenario.RequestId);
        var path = SplitReferralApiHelpers.Path(scenario.RequestId, scenario.NeedId);
        await SplitReferralApiAssertions.RejectAsync(coordinator, path, Guid.NewGuid(), [SplitReferralApiHelpers.Leg(scenario, 1m)]);
        await SplitReferralApiAssertions.RejectAsync(coordinator, SplitReferralApiHelpers.Path(scenario.RequestId, Guid.NewGuid()), version, [SplitReferralApiHelpers.Leg(scenario, 1m)]);
        await SplitReferralApiAssertions.RejectAsync(coordinator, SplitReferralApiHelpers.Path(scenario.RequestId, scenario.RejectedNeedId), version, [SplitReferralApiHelpers.Leg(scenario, 1m)]);
        await SplitReferralApiAssertions.RejectAsync(coordinator, path, version, [SplitReferralApiHelpers.Leg(scenario, 1m) with { StandardVersionId = Guid.NewGuid() }]);
        await SplitReferralApiAssertions.RejectAsync(coordinator, path, version, [SplitReferralApiHelpers.Leg(scenario, 2m)]);
        await SplitReferralApiAssertions.ReferralsAsync(fixture, scenario, 0, 0m);
    }

}
