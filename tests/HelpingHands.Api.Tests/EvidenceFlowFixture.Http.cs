using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowFixture
{
    public static async Task<JsonElement> PostJsonAsync(HttpClient client, string route, object value, HttpStatusCode status)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, route) { Content = JsonContent.Create(value) };
        request.Headers.Add("X-CSRF-TOKEN", await TokenAsync(client));
        using var response = await client.SendAsync(request);
        Assert.Equal(status, response.StatusCode);
        return status == HttpStatusCode.NoContent ? default : await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<string> TokenAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/api/v1/auth/antiforgery");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("requestToken").GetString()!;
    }

    public async Task LoginAsync(HttpClient client, string email) =>
        await PostJsonAsync(client, "/api/v1/auth/login", new { email, Password }, HttpStatusCode.NoContent);
}
