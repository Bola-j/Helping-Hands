using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    public static async Task<HttpResponseMessage> RegisterAsync(HttpClient client, string name, string email, string password)
    {
        using var tokenResponse = await client.GetAsync("/api/v1/auth/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/register")
        { Content = JsonContent.Create(new { DisplayName = name, Email = email, Password = password }) };
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> LoginRegisteredAsync(HttpClient client, string email, string password) =>
        await PostAsync(client, "/api/v1/auth/login", new { Email = email, Password = password });
}
