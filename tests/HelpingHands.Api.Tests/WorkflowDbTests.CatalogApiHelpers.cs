using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private static async Task<HttpResponseMessage> CatalogPublishAsync(HttpClient client, string path)
    {
        using var resource = await client.GetAsync(path);
        var etag = resource.Headers.ETag!.Tag;
        var token = await client.GetFromJsonAsync<JsonElement>("/api/v1/auth/antiforgery");
        using var request = new HttpRequestMessage(HttpMethod.Post, path + "/publish") { Content = JsonContent.Create(new { }) };
        request.Headers.TryAddWithoutValidation("If-Match", etag);
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }
}
