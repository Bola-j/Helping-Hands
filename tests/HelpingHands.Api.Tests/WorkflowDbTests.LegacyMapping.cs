using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task LegacyStandardMapping_RequiresStaffAndCurrentRequestVersion()
    {
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var scope = fixture.Factory.Services.CreateScope();
        var (_, _, _, standard) = await PublishedCatalogAsync(scope.ServiceProvider.GetRequiredService<HelpingHands.Api.Data.HelpingHandsDbContext>());
        var request = await WorkflowDbFixture.AcceptedRequestAsync(beneficiary, coordinator);
        var path = $"/api/v1/requests/{request.GetProperty("id").GetGuid()}/legacy-standard";
        var version = request.GetProperty("version").GetGuid();
        using var forbidden = await WorkflowDbFixture.PostAsync(beneficiary, path, new { StandardVersionId = standard.Id, Version = version });
        using var stale = await WorkflowDbFixture.PostAsync(coordinator, path, new { StandardVersionId = standard.Id, Version = Guid.NewGuid() });
        using var missing = await WorkflowDbFixture.PostAsync(coordinator, path, new { StandardVersionId = Guid.NewGuid(), Version = version });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, missing.StatusCode);
    }

    [Fact]
    public async Task LegacyStandardMapping_RecordsSelectionAndCannotBeReplaced()
    {
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHands.Api.Data.HelpingHandsDbContext>();
        var (_, _, _, selected) = await PublishedCatalogAsync(db);
        var (_, _, _, other) = await PublishedCatalogAsync(db);
        var request = await WorkflowDbFixture.AcceptedRequestAsync(beneficiary, coordinator);
        var path = $"/api/v1/requests/{request.GetProperty("id").GetGuid()}/legacy-standard";
        using var response = await WorkflowDbFixture.PostAsync(coordinator, path, new { StandardVersionId = selected.Id, Version = request.GetProperty("version").GetGuid() });
        var mapped = await WorkflowDbFixture.JsonAsync(response, HttpStatusCode.OK);
        Assert.Equal(selected.Id, mapped.GetProperty("legacyStandardVersionId").GetGuid());
        Assert.NotEqual(request.GetProperty("version").GetGuid(), mapped.GetProperty("version").GetGuid());
        Assert.NotEqual(System.Text.Json.JsonValueKind.Null, mapped.GetProperty("legacyMappedAt").ValueKind);
        using var historyResponse = await coordinator.GetAsync($"/api/v1/requests/{request.GetProperty("id").GetGuid()}/history");
        var history = await WorkflowDbFixture.JsonAsync(historyResponse, HttpStatusCode.OK);
        Assert.Contains(history.EnumerateArray(), entry => entry.GetProperty("action").GetString() == "LegacyStandardMapped");
        using var retry = await WorkflowDbFixture.PostAsync(coordinator, path, new { StandardVersionId = other.Id, Version = mapped.GetProperty("version").GetGuid() });
        Assert.Equal(HttpStatusCode.Conflict, retry.StatusCode);
    }
}
