using System.Net;

namespace HelpingHands.Api.Tests;

[Trait("Category", "PostgresIntegration")]
public sealed partial class SessionCookieTests(SessionCookieFixture fixture) : IClassFixture<SessionCookieFixture>
{
    private readonly SessionCookieFixture fixture = fixture;

    [Fact]
    public async Task Login_IssuesSecureAuthenticationCookie()
    {
        using var client = fixture.Client();
        using var response = await fixture.LoginAsync(client);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        using var me = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }
}
