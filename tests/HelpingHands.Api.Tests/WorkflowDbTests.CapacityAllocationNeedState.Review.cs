using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private static async Task ReviewNeedsAsync(HttpClient coordinator, MultiNeedAllocationScenario scenario)
    {
        using var acceptedResponse = await WorkflowDbFixture.PostAsync(coordinator,
            $"/api/v1/requests/{scenario.RequestId}/needs/{scenario.AcceptedNeedId}/decisions",
            new { Decision = "Accepted", Category = "Goods", Version = scenario.RequestVersion });
        var accepted = await WorkflowDbFixture.JsonAsync(acceptedResponse, HttpStatusCode.OK);
        using var rejectedResponse = await WorkflowDbFixture.PostAsync(coordinator,
            $"/api/v1/requests/{scenario.RequestId}/needs/{scenario.RejectedNeedId}/decisions",
            new { Decision = "Rejected", Category = "Money", Reason = "Fictional scope decision",
                Version = accepted.GetProperty("version").GetGuid() });
        await WorkflowDbFixture.JsonAsync(rejectedResponse, HttpStatusCode.OK);
    }
}
