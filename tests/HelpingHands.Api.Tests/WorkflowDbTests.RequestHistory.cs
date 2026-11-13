using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task RejectionHistory_RecordsReasonForStaffButConcealsItFromRequester()
    {
        using var requester = await fixture.LoginAsync("Requester");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        var request = await SubmittedRequestAsync(requester);
        var rejected = await ReviewRequestAsync(coordinator, request, "Rejected", "Fictional staff reason");
        Assert.Equal("Rejected", rejected.GetProperty("status").GetString());
        var path = $"/api/v1/requests/{request.GetProperty("id").GetGuid()}/history";
        using var staffResponse = await coordinator.GetAsync(path);
        var staff = await WorkflowDbFixture.JsonAsync(staffResponse, HttpStatusCode.OK);
        var decision = staff.EnumerateArray().Single(item => item.GetProperty("action").GetString() == "Rejected");
        Assert.Equal("Fictional staff reason", decision.GetProperty("reason").GetString());
        using var requesterResponse = await requester.GetAsync(path);
        var requesterHistory = await WorkflowDbFixture.JsonAsync(requesterResponse, HttpStatusCode.OK);
        Assert.All(requesterHistory.EnumerateArray(), item => Assert.False(item.TryGetProperty("reason", out _)));
    }
    [Fact]
    public async Task ClarificationHistory_RecordsRequestReviewAndResponse()
    {
        using var requester = await fixture.LoginAsync("Requester");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        var request = await SubmittedRequestAsync(requester);
        var clarification = await ReviewRequestAsync(coordinator, request, "Clarification", "Fictional clarification");
        var id = request.GetProperty("id").GetGuid();
        using var updated = await UpdateRequestAsync(requester, id, clarification.GetProperty("version").GetGuid());
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        using var historyResponse = await coordinator.GetAsync($"/api/v1/requests/{id}/history");
        var history = await WorkflowDbFixture.JsonAsync(historyResponse, HttpStatusCode.OK);
        var actions = history.EnumerateArray().Select(item => item.GetProperty("action").GetString()).ToArray();
        Assert.Equal(new[] { "Submitted", "Clarification", "ClarificationSubmitted" }, actions);
    }
}
