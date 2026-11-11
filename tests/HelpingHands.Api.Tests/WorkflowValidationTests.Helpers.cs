using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowValidationTests
{
    private static Dictionary<string, string?> Settings() => new()
    {
        ["ConnectionStrings:HelpingHands"] = "Host=127.0.0.1;Port=1;Database=unused;Username=unused;Timeout=1",
        ["EvidenceStorage:ServiceUrl"] = "http://127.0.0.1:1",
        ["EvidenceStorage:Bucket"] = "test-bucket",
        ["EvidenceStorage:AccessKey"] = "fictional",
        ["EvidenceStorage:SecretKey"] = "fictional"
    };

    private HttpClient Client() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    private static async Task<HttpResponseMessage> PostWithToken(HttpClient client, string path, object payload)
    {
        using var tokenResponse = await client.GetAsync("/api/v1/auth/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(payload) };
        request.Headers.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.SendAsync(request);
    }
}
