using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CatalogVersionCursorPagesWithoutRepeatingAndRejectsMalformedCursor()
    {
        using var coordinator = await fixture.LoginAsync("Coordinator");
        var category = await CatalogPostAsync(coordinator, "/api/v1/catalog/categories", new { Code = Guid.NewGuid().ToString("N") }, HttpStatusCode.Created);
        var id = category.GetProperty("id").GetGuid();
        var path = $"/api/v1/catalog/categories/{id}/versions";
        var expected = new HashSet<Guid>();
        for (var i = 0; i < 3; i++)
            expected.Add((await CatalogPostAsync(coordinator, path, new { Name = $"Fictional {i}" }, HttpStatusCode.Created)).GetProperty("id").GetGuid());
        var actual = new List<Guid>();
        string? cursor = null;
        do
        {
            var page = await coordinator.GetFromJsonAsync<JsonElement>($"{path}?limit=1{(cursor is null ? "" : "&cursor=" + Uri.EscapeDataString(cursor))}");
            actual.AddRange(page.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("id").GetGuid()));
            cursor = page.GetProperty("nextCursor").GetString();
        } while (cursor is not null);
        Assert.Equal(3, actual.Count);
        Assert.True(expected.SetEquals(actual));
        Assert.Equal(HttpStatusCode.BadRequest, (await coordinator.GetAsync(path + "?cursor=invalid")).StatusCode);
    }

    [Fact]
    public async Task CategoryCodeCanBeEditedOnlyBeforePublication()
    {
        using var coordinator = await fixture.LoginAsync("Coordinator");
        var category = await CatalogPostAsync(coordinator, "/api/v1/catalog/categories", new { Code = Guid.NewGuid().ToString("N") }, HttpStatusCode.Created);
        var id = category.GetProperty("id").GetGuid();
        using var changed = await CatalogSendAsync(coordinator, HttpMethod.Put, $"/api/v1/catalog/categories/{id}", new { Code = Guid.NewGuid().ToString("N") });
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        var service = await CatalogPostAsync(coordinator, $"/api/v1/catalog/categories/{id}/versions", new { Name = "Food" }, HttpStatusCode.Created);
        using var published = await CatalogPublishAsync(coordinator, $"/api/v1/catalog/service-versions/{service.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.OK, published.StatusCode);
        using var forbidden = await CatalogSendAsync(coordinator, HttpMethod.Put, $"/api/v1/catalog/categories/{id}", new { Code = Guid.NewGuid().ToString("N") });
        Assert.Equal(HttpStatusCode.Conflict, forbidden.StatusCode);
    }
}
