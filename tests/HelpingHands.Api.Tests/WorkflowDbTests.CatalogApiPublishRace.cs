using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task StandardPublishRejectsETagAfterComponentsChange()
    {
        using var coordinator = await fixture.LoginAsync("Coordinator");
        var category = await CatalogPostAsync(coordinator, "/api/v1/catalog/categories", new { Code = Guid.NewGuid().ToString("N") }, HttpStatusCode.Created);
        var service = await CatalogPostAsync(coordinator, $"/api/v1/catalog/categories/{category.GetProperty("id").GetGuid()}/versions", new { Name = "Food" }, HttpStatusCode.Created);
        var serviceId = service.GetProperty("id").GetGuid();
        using var servicePublished = await CatalogPublishAsync(coordinator, $"/api/v1/catalog/service-versions/{serviceId}");
        Assert.Equal(HttpStatusCode.OK, servicePublished.StatusCode);
        var standard = await CatalogPostAsync(coordinator, $"/api/v1/catalog/service-versions/{serviceId}/standards", new { Name = "Parcel" }, HttpStatusCode.Created);
        var version = await CatalogPostAsync(coordinator, $"/api/v1/catalog/standards/{standard.GetProperty("id").GetGuid()}/versions", new { EffectiveAt = DateTimeOffset.UtcNow }, HttpStatusCode.Created);
        var path = $"/api/v1/catalog/standard-versions/{version.GetProperty("id").GetGuid()}";
        await CatalogPostAsync(coordinator, path + "/components", new { Name = "Rice", Quantity = 1, Unit = "kg" }, HttpStatusCode.Created);
        using var preview = await coordinator.GetAsync(path);
        var staleTag = preview.Headers.ETag!.Tag;
        var reviewed = await preview.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(reviewed.GetProperty("components").EnumerateArray());
        Assert.Equal("Rice", reviewed.GetProperty("components")[0].GetProperty("name").GetString());
        await CatalogPostAsync(coordinator, path + "/components", new { Name = "Beans", Quantity = 1, Unit = "kg" }, HttpStatusCode.Created);
        var token = await coordinator.GetFromJsonAsync<JsonElement>("/api/v1/auth/antiforgery");
        using var publish = new HttpRequestMessage(HttpMethod.Post, path + "/publish") { Content = JsonContent.Create(new { }) };
        publish.Headers.TryAddWithoutValidation("If-Match", staleTag);
        publish.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        using var rejected = await coordinator.SendAsync(publish);
        Assert.Equal(HttpStatusCode.Conflict, rejected.StatusCode);
        using var refreshed = await CatalogPublishAsync(coordinator, path);
        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);
        var published = await refreshed.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, published.GetProperty("components").GetArrayLength());
    }
}
