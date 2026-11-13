using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private static async Task AssertRequestAndHistoryAsync(HttpClient coordinator, JsonElement referral)
    {
        var request = referral.GetProperty("requestId").GetGuid();
        using var response = await coordinator.GetAsync($"/api/v1/requests/{request}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        // This records the current demo behavior; explicit closure remains P1-55.
        Assert.Equal("Completed", payload.GetProperty("status").GetString());
        using var historyResponse = await coordinator.GetAsync($"/api/v1/referrals/{Id(referral)}/history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        var history = await historyResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(history.EnumerateArray(), item => item.GetProperty("action").GetString() == "Assigned");
        Assert.Contains(history.EnumerateArray(), item => item.GetProperty("action").GetString() == "OutcomeAccepted");
    }
}
