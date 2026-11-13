using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private static Task<HttpResponseMessage> DecideAsync(HttpClient client, Guid id, Guid version, string decision, string? reason) =>
        WorkflowDbFixture.PostAsync(client, $"/api/v1/recipients/{id}/decisions", new { Decision = decision, Reason = reason, Version = version });

    private static async Task AssertDecisionFailuresAsync(HttpClient applicant, HttpClient coordinator, Guid id, Guid version)
    {
        using var forbidden = await DecideAsync(applicant, id, version, "Approved", null);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        using var stale = await DecideAsync(coordinator, id, Guid.NewGuid(), "Approved", null);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var noReason = await DecideAsync(coordinator, id, version, "Rejected", " ");
        Assert.Equal(HttpStatusCode.BadRequest, noReason.StatusCode);
    }

    private async Task AssertDecisionDetailAsync(HttpClient applicant, HttpClient coordinator, HttpClient outsider, Guid id)
    {
        using var ownerResponse = await applicant.GetAsync($"/api/v1/recipients/{id}");
        var detail = await WorkflowDbFixture.JsonAsync(ownerResponse, HttpStatusCode.OK);
        Assert.Equal("Rejected", detail.GetProperty("approvalStatus").GetString());
        Assert.False(detail.GetProperty("active").GetBoolean());
        var decision = Assert.Single(detail.GetProperty("decisions").EnumerateArray());
        Assert.Equal("Fictional reason", decision.GetProperty("reason").GetString());
        Assert.Equal(fixture.UserIds["Coordinator"], decision.GetProperty("actorUserId").GetGuid());
        Assert.True(decision.GetProperty("decidedAt").GetDateTimeOffset() <= DateTimeOffset.UtcNow);
        using var staffResponse = await coordinator.GetAsync($"/api/v1/recipients/{id}");
        Assert.Equal(HttpStatusCode.OK, staffResponse.StatusCode);
        using var hidden = await outsider.GetAsync($"/api/v1/recipients/{id}");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        using var anonymous = fixture.Client();
        using var unauthenticated = await anonymous.GetAsync($"/api/v1/recipients/{id}");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);
    }
}
