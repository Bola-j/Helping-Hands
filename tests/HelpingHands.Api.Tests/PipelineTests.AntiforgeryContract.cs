using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class PipelineTests
{
    [Fact]
    public async Task AntiforgeryToken_IsAnonymousAndIssuesProtectedCookie()
    {
        using var client = AntiforgeryClient();
        using var response = await client.GetAsync("/api/v1/auth/antiforgery");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(payload.GetProperty("requestToken").GetString()));
        Assert.Equal("X-CSRF-TOKEN", payload.GetProperty("headerName").GetString());
        Assert.Contains("no-store", response.Headers.CacheControl!.ToString());
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MatchingAntiforgeryToken_AllowsLoginHandlerToRun()
    {
        using var client = AntiforgeryClient();
        var token = await GetAntiforgeryTokenAsync(client);
        using var response = await LoginWithTokenAsync(client, token);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }
}
