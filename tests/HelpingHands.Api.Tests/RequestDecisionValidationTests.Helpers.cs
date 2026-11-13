using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HelpingHands.Api.Tests;

public sealed partial class RequestDecisionValidationTests
{
    private HttpClient Client() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    private static async Task<HttpResponseMessage> DecideAsync(HttpClient client, object payload)
    {
        using var tokenResponse = await client.GetAsync("/api/v1/auth/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/requests/{Guid.NewGuid()}/decisions")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }
}
