using System.Net;
using System.Net.Http.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class PipelineTests
{
    [Fact]
    public async Task ExpectedUser_RejectsAnonymousUnsafeRequest()
    {
        using var client = AntiforgeryClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login") { Content = JsonContent.Create(new { Email = "", Password = "" }) };
        request.Headers.Add("X-CSRF-TOKEN", await GetAntiforgeryTokenAsync(client));
        request.Headers.Add("X-Expected-User-Id", "d3cf9e0c-f849-4e6c-9f52-204f030b7356");
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(response.Headers.Contains("X-Session-Changed"));
    }

    [Fact]
    public async Task ExpectedUser_RejectsAnonymousSafeRequest()
    {
        using var client = AntiforgeryClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/antiforgery");
        request.Headers.TryAddWithoutValidation("X-Expected-User-Id", "not-a-guid");
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("true", Assert.Single(response.Headers.GetValues("X-Session-Changed")));
    }

    [Fact]
    public async Task ExpectedUser_ValidHeaderWithAnonymousSafeRequestIsUnauthorized()
    {
        using var client = AntiforgeryClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/antiforgery");
        request.Headers.Add("X-Expected-User-Id", "d3cf9e0c-f849-4e6c-9f52-204f030b7356");
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(response.Headers.Contains("X-Session-Changed"));
    }
}
