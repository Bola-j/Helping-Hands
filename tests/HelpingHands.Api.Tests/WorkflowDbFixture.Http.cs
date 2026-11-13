using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    public async Task<HttpClient> LoginAsync(string role)
    {
        var client = Client();
        if (SessionCookies.TryGetValue(role, out var existing))
        {
            client.DefaultRequestHeaders.Add("Cookie", existing);
            return client;
        }
        using var response = await PostAsync(client, "/api/v1/auth/login", new { Email = Emails[role], Password });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var cookie = response.Headers.GetValues("Set-Cookie").Single(value =>
            value.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
        SessionCookies.Add(role, cookie.Split(';')[0]);
        return client;
    }

    public static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, object body)
    {
        using var tokenResponse = await client.GetAsync("/api/v1/auth/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }

    public static async Task<JsonElement> JsonAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
