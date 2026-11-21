using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private static async Task AssertLegacyClosureBlockedAsync(HttpClient coordinator, JsonElement referral)
    {
        var requestId = referral.GetProperty("requestId").GetGuid();
        using var current = await coordinator.GetAsync($"/api/v1/requests/{requestId}");
        var request = await current.Content.ReadFromJsonAsync<JsonElement>();
        await EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/requests/{requestId}/closure",
            new { Version = request.GetProperty("version").GetGuid() }, HttpStatusCode.Conflict);
        using var unchanged = await coordinator.GetAsync($"/api/v1/requests/{requestId}");
        var latest = await unchanged.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Open", latest.GetProperty("status").GetString());
        Assert.Equal(request.GetProperty("version").GetGuid(), latest.GetProperty("version").GetGuid());
    }

    private static async Task AssertLegacyRequestClosedAsync(HttpClient coordinator, Guid requestId)
    {
        using var current = await coordinator.GetAsync($"/api/v1/requests/{requestId}");
        var request = await current.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Open", request.GetProperty("status").GetString());
        await EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/requests/{requestId}/closure",
            new { Version = request.GetProperty("version").GetGuid() }, HttpStatusCode.NoContent);
        using var completedResponse = await coordinator.GetAsync($"/api/v1/requests/{requestId}");
        var completed = await completedResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Completed", completed.GetProperty("status").GetString());
        using var historyResponse = await coordinator.GetAsync($"/api/v1/requests/{requestId}/history");
        var history = await historyResponse.Content.ReadFromJsonAsync<JsonElement>();
        var closed = history.EnumerateArray().Single(item => item.GetProperty("action").GetString() == "RequestClosed");
        Assert.NotEqual(Guid.Empty, closed.GetProperty("actorUserId").GetGuid());
        Assert.NotEqual(default, closed.GetProperty("occurredAt").GetDateTimeOffset());
        await AssertLegacyReferralBlockedAsync(coordinator, requestId, completed.GetProperty("version").GetGuid());
    }

    private static async Task AssertLegacyReferralBlockedAsync(HttpClient coordinator, Guid requestId, Guid version)
    {
        await EvidenceFlowFixture.PostJsonAsync(coordinator, $"/api/v1/requests/{requestId}/referrals", new
        {
            RecipientId = Guid.NewGuid(), AssignedQuantity = 1m, ScopeDescription = "Fictional post-closure allocation",
            Version = version, ServiceVersionId = Guid.NewGuid(), StandardVersionId = Guid.NewGuid()
        }, HttpStatusCode.Conflict);
    }
}
