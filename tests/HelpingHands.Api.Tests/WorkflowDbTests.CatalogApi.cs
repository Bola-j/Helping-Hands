using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CatalogApisPublishAndExposeOnlyCurrentVersionsToParticipants()
    {
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var category = await CatalogPostAsync(coordinator, "/api/v1/catalog/categories", new { Code = Guid.NewGuid().ToString("N") }, HttpStatusCode.Created);
        var categoryId = category.GetProperty("id").GetGuid();
        var service = await CatalogPostAsync(coordinator, $"/api/v1/catalog/categories/{categoryId}/versions", new { Name = "Fictional food" }, HttpStatusCode.Created);
        var serviceId = service.GetProperty("id").GetGuid();
        var standard = await CatalogPostAsync(coordinator, $"/api/v1/catalog/service-versions/{serviceId}/standards", new { Name = "Fictional parcel" }, HttpStatusCode.Created);
        var standardId = standard.GetProperty("id").GetGuid();
        var version = await CatalogPostAsync(coordinator, $"/api/v1/catalog/standards/{standardId}/versions", new { EffectiveAt = DateTimeOffset.UtcNow.AddDays(-1) }, HttpStatusCode.Created);
        var versionId = version.GetProperty("id").GetGuid();
        using var denied = await WorkflowDbFixture.PostAsync(beneficiary, "/api/v1/catalog/categories", new { Code = "NO" });
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await beneficiary.GetAsync($"/api/v1/catalog/standards/{standardId}/versions")).StatusCode);
        using var empty = await CatalogPublishAsync(coordinator, $"/api/v1/catalog/standard-versions/{versionId}");
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        using var invalid = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/catalog/standard-versions/{versionId}/components", new { Name = "Rice", Quantity = 0, Unit = "kg" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var component = await CatalogPostAsync(coordinator, $"/api/v1/catalog/standard-versions/{versionId}/components", new { Name = "Rice", Quantity = 1.25m, Unit = "kg" }, HttpStatusCode.Created);
        using var beforeService = await CatalogPublishAsync(coordinator, $"/api/v1/catalog/standard-versions/{versionId}");
        Assert.Equal(HttpStatusCode.Conflict, beforeService.StatusCode);
        using var publishedService = await CatalogPublishAsync(coordinator, $"/api/v1/catalog/service-versions/{serviceId}");
        Assert.Equal(HttpStatusCode.OK, publishedService.StatusCode);
        using var publishedStandard = await CatalogPublishAsync(coordinator, $"/api/v1/catalog/standard-versions/{versionId}");
        Assert.Equal(HttpStatusCode.OK, publishedStandard.StatusCode);
        using var edit = await CatalogSendAsync(coordinator, HttpMethod.Put, $"/api/v1/catalog/components/{component.GetProperty("id").GetGuid()}", new { Name = "Rice", Quantity = 2, Unit = "kg" });
        Assert.Equal(HttpStatusCode.Conflict, edit.StatusCode);
        var choices = new List<JsonElement>();
        string? cursor = null;
        do
        {
            var page = await beneficiary.GetFromJsonAsync<JsonElement>($"/api/v1/catalog/current?limit=100{(cursor is null ? "" : "&cursor=" + Uri.EscapeDataString(cursor))}");
            choices.AddRange(page.GetProperty("items").EnumerateArray());
            cursor = page.GetProperty("nextCursor").GetString();
        } while (cursor is not null);
        Assert.Contains(choices, x => x.GetProperty("standardVersionId").GetGuid() == versionId && x.GetProperty("components")[0].GetProperty("quantity").GetDecimal() == 1.25m);
        var history = await coordinator.GetFromJsonAsync<JsonElement>($"/api/v1/catalog/standards/{standardId}/versions");
        Assert.Equal(versionId, history.GetProperty("items")[0].GetProperty("id").GetGuid());
    }

    private static async Task<JsonElement> CatalogPostAsync(HttpClient client, string path, object input, HttpStatusCode expected = HttpStatusCode.OK)
    {
        using var response = await WorkflowDbFixture.PostAsync(client, path, input);
        return await WorkflowDbFixture.JsonAsync(response, expected);
    }

    private static async Task<HttpResponseMessage> CatalogSendAsync(HttpClient client, HttpMethod method, string path, object input)
    {
        var token = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/antiforgery");
        using var resource = await client.GetAsync(path);
        var etag = resource.Headers.ETag!.Tag;
        using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(input) };
        request.Headers.TryAddWithoutValidation("If-Match", etag);
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }
}
