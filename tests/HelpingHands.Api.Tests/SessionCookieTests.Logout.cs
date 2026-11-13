using System.Net;
using System.Net.Http.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class SessionCookieTests
{
    [Fact]
    public async Task LogoutRevokesSessionAndClearsAuthenticationCookie()
    {
        using var client = fixture.Client();
        using var login = await fixture.LoginAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout") { Content = JsonContent.Create(new { }) };
        request.Headers.Add("X-CSRF-TOKEN", await SessionCookieFixture.TokenAsync(client));
        using var logout = await client.SendAsync(request);
        using var profile = await client.GetAsync("/api/v1/auth/me");
        var clearedCookie = Assert.Single(logout.Headers.GetValues("Set-Cookie"), value => value.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Contains("expires=", clearedCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.Unauthorized, profile.StatusCode);
    }

    [Fact]
    public async Task AnonymousLogoutReturnsUnauthorizedWithoutRedirect()
    {
        using var client = fixture.Client();
        using var logout = await client.PostAsJsonAsync("/api/v1/auth/logout", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, logout.StatusCode);
        Assert.Null(logout.Headers.Location);
    }
}
