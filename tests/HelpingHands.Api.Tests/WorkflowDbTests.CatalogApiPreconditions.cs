using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CatalogDraftEditsRequireCurrentETag()
    {
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var unrelated = await fixture.LoginAsync("Unrelated");
        Assert.Equal(HttpStatusCode.Forbidden, (await unrelated.GetAsync("/api/v1/catalog/current")).StatusCode);
        var category = await CatalogPostAsync(coordinator, "/api/v1/catalog/categories", new { Code = Guid.NewGuid().ToString("N") }, HttpStatusCode.Created);
        var path = $"/api/v1/catalog/categories/{category.GetProperty("id").GetGuid()}/versions";
        using var created = await WorkflowDbFixture.PostAsync(coordinator, path, new { Name = "Initial" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        var service = await created.Content.ReadFromJsonAsync<JsonElement>();
        var servicePath = $"/api/v1/catalog/service-versions/{service.GetProperty("id").GetGuid()}";
        using var initial = await coordinator.GetAsync(servicePath);
        var oldTag = initial.Headers.ETag!.Tag;
        using var changed = await CatalogSendAsync(coordinator, HttpMethod.Put, servicePath, new { Name = "Changed" });
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        var token = await coordinator.GetFromJsonAsync<JsonElement>("/api/v1/auth/antiforgery");
        using var request = new HttpRequestMessage(HttpMethod.Put, servicePath) { Content = JsonContent.Create(new { Name = "Stale" }) };
        request.Headers.TryAddWithoutValidation("If-Match", oldTag);
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        using var response = await coordinator.SendAsync(request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var current = await coordinator.GetFromJsonAsync<JsonElement>(servicePath);
        Assert.Equal("Changed", current.GetProperty("name").GetString());
    }
}
