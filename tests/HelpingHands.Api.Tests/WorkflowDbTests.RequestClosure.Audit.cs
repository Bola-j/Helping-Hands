using System.Net;
using System.Net.Http.Json;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task AssertClosedRequestAuditAsync(Guid requestId, Guid oldVersion, Guid[] needIds, Guid expectedActor, HttpClient coordinator)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var request = await database.Requests.SingleAsync(item => item.Id == requestId);
        var history = await database.DecisionHistory.SingleAsync(item => item.RequestId == requestId && item.Action == "RequestClosed");
        Assert.Equal("Completed", request.Status);
        Assert.NotEqual(oldVersion, request.Version);
        Assert.Equal(expectedActor, history.ActorUserId);
        Assert.InRange(history.OccurredAt, DateTimeOffset.UtcNow.AddMinutes(-2), DateTimeOffset.UtcNow.AddMinutes(2));
        Assert.Equal(2, await database.RequestNeeds.CountAsync(item => needIds.Contains(item.Id) && item.Status == "Accepted"));
        using var historyResponse = await coordinator.GetAsync($"/api/v1/requests/{requestId}/history");
        var historyPayload = await historyResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var visibleEvent = historyPayload.EnumerateArray().Single(item => item.GetProperty("action").GetString() == "RequestClosed");
        Assert.Equal(expectedActor, visibleEvent.GetProperty("actorUserId").GetGuid());
        Assert.Equal(history.OccurredAt, visibleEvent.GetProperty("occurredAt").GetDateTimeOffset());
        using var replay = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{requestId}/closure", new { Version = oldVersion });
        Assert.Equal(HttpStatusCode.Conflict, replay.StatusCode);
    }
}
