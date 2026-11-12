using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class SessionCookieTests
{
    [Fact]
    public async Task Session_ExpiresAfterEightHoursWithoutSlidingRenewal()
    {
        using var client = fixture.Client();
        using var login = await fixture.LoginAsync(client);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        fixture.Clock.Advance(TimeSpan.FromHours(4) + TimeSpan.FromMinutes(1));
        using var halfway = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, halfway.StatusCode);
        Assert.False(halfway.Headers.TryGetValues("Set-Cookie", out var cookies) &&
            cookies.Any(value => value.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal)));
        fixture.Clock.Advance(TimeSpan.FromHours(3) + TimeSpan.FromMinutes(58));
        using var beforeExpiry = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, beforeExpiry.StatusCode);
        fixture.Clock.Advance(TimeSpan.FromMinutes(2));
        using var expired = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, expired.StatusCode);
        Assert.Null(expired.Headers.Location);
    }
}
