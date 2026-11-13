using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task ConcurrentCatalogVersionCreationAllocatesDistinctVersions()
    {
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var admin = await fixture.LoginAsync("Administrator");
        var category = await CatalogPostAsync(coordinator, "/api/v1/catalog/categories", new { Code = Guid.NewGuid().ToString("N") }, HttpStatusCode.Created);
        var path = $"/api/v1/catalog/categories/{category.GetProperty("id").GetGuid()}/versions";
        var results = await Task.WhenAll(WorkflowDbFixture.PostAsync(coordinator, path, new { Name = "Food one" }), WorkflowDbFixture.PostAsync(admin, path, new { Name = "Food two" }));
        using var first = results[0];
        using var second = results[1];
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var firstJson = await first.Content.ReadFromJsonAsync<JsonElement>();
        var secondJson = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(new[] { 1, 2 }, new[] { firstJson.GetProperty("versionNumber").GetInt32(), secondJson.GetProperty("versionNumber").GetInt32() }.Order().ToArray());
    }
}
