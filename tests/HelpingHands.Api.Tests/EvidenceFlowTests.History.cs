using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private static async Task AssertRequestAndHistoryAsync(HttpClient coordinator, JsonElement referral)
    {
        var request = referral.GetProperty("requestId").GetGuid();
        await AssertLegacyRequestClosedAsync(coordinator, request);
        using var historyResponse = await coordinator.GetAsync($"/api/v1/referrals/{Id(referral)}/history");
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        var history = await historyResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(history.EnumerateArray(), item => item.GetProperty("action").GetString() == "Assigned");
        Assert.Contains(history.EnumerateArray(), item => item.GetProperty("action").GetString() == "OutcomeAccepted");
    }
}
