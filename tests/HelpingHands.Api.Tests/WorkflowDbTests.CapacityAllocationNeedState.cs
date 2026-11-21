namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task SplitAllocation_PreservesNeedDecisionsAndAllowsOutcomeReview()
    {
        var scenario = await PrepareAcceptedNeedAllocationScenarioAsync();
        var allocation = await AllocateNeedAsync(scenario);
        Assert.True(allocation.Succeeded);
        await AssertNeedStateAsync(scenario);
        await AssertOutcomeReviewAllowedAsync(scenario, allocation.Legs.Single().ReferralReference);
    }
}
