using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HelpingHands.Api.Tests;

public sealed partial class PipelineTests
{
    [Fact]
    public async Task AntiforgeryEndpoint_ReturnsTokenAndSecureCookie()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        var response = await client.GetAsync("/api/v1/auth/antiforgery");
        var payload = await response.Content.ReadFromJsonAsync<AntiforgeryResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(payload?.RequestToken));
        Assert.Equal("X-CSRF-TOKEN", payload?.HeaderName);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), value => value.Contains("secure", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record AntiforgeryResponse(string? RequestToken, string? HeaderName);
}
