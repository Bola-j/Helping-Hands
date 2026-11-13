using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task NeedReviewTracksMixedDecisionsClarificationAndHistory()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (_, _, _, first) = await PublishedCatalogAsync(db);
        var (_, _, _, second) = await PublishedCatalogAsync(db);
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            Assert.Equal(NeedCreationResult.Created, await scope.ServiceProvider.GetRequiredService<NeedVersionService>().AddNeedsAsync(request.Id, [new(first.Id, 1), new(second.Id, 2)]));
            await tx.CommitAsync();
        }
        var needs = await db.RequestNeeds.AsNoTracking().Where(x => x.RequestId == request.Id).OrderBy(x => x.Quantity).ToListAsync();
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        using var unrelated = await fixture.LoginAsync("Unrelated");
        var basePath = $"/api/v1/requests/{request.Id}/needs";
        using var denied = await WorkflowDbFixture.PostAsync(unrelated, $"{basePath}/{needs[0].Id}/decisions", new { Decision = "Accepted", Category = "Goods", Version = request.Version });
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var firstDecision = await WorkflowDbFixture.PostAsync(coordinator, $"{basePath}/{needs[0].Id}/decisions", new { Decision = "Accepted", Category = "Goods", Version = request.Version });
        var pending = await WorkflowDbFixture.JsonAsync(firstDecision, HttpStatusCode.OK);
        Assert.Equal("InReview", pending.GetProperty("status").GetString());
        var version = pending.GetProperty("version").GetGuid();
        using var stale = await WorkflowDbFixture.PostAsync(coordinator, $"{basePath}/{needs[1].Id}/decisions", new { Decision = "Rejected", Category = "Money", Reason = "Fictional reason", Version = request.Version });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var clarification = await WorkflowDbFixture.PostAsync(coordinator, $"{basePath}/{needs[1].Id}/decisions", new { Decision = "Clarification", Reason = "Please explain", Version = version });
        var asked = await WorkflowDbFixture.JsonAsync(clarification, HttpStatusCode.OK);
        Assert.Equal("Clarification", asked.GetProperty("status").GetString());
        using var response = await WorkflowDbFixture.PostAsync(beneficiary, $"{basePath}/{needs[1].Id}/clarifications", new { Explanation = "Fictional details", Version = asked.GetProperty("version").GetGuid() });
        var resubmitted = await WorkflowDbFixture.JsonAsync(response, HttpStatusCode.OK);
        using var missingReason = await WorkflowDbFixture.PostAsync(coordinator, $"{basePath}/{needs[1].Id}/decisions", new { Decision = "Rejected", Category = "Money", Version = resubmitted.GetProperty("version").GetGuid() });
        Assert.Equal(HttpStatusCode.BadRequest, missingReason.StatusCode);
        using var reject = await WorkflowDbFixture.PostAsync(coordinator, $"{basePath}/{needs[1].Id}/decisions", new { Decision = "Rejected", Category = "Money", Reason = "Not eligible", Version = resubmitted.GetProperty("version").GetGuid() });
        var completed = await WorkflowDbFixture.JsonAsync(reject, HttpStatusCode.OK);
        Assert.Equal("PartiallyAccepted", completed.GetProperty("status").GetString());
        using var history = await coordinator.GetAsync($"{basePath}/{needs[1].Id}/history");
        var events = await history.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, events.GetArrayLength());
        Assert.Equal(fixture.UserIds["Coordinator"], events[0].GetProperty("actorUserId").GetGuid());
        Assert.Equal(fixture.UserIds["Beneficiary"], events[1].GetProperty("actorUserId").GetGuid());
    }
}
