using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class SessionCookieTests
{
    [Fact]
    public async Task InvalidCredentialsDoNotIssueAuthenticationCookie()
    {
        using var client = fixture.Client();
        using var badPassword = await fixture.LoginAsync(client, fixture.Email, "Wrong-fictional-password");
        using var unknownUser = await fixture.LoginAsync(client, "missing@example.test", fixture.Password);
        Assert.Equal(HttpStatusCode.Unauthorized, badPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);
        Assert.False(badPassword.Headers.Contains("Set-Cookie"));
        Assert.False(unknownUser.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task RepeatedInvalidCredentialsLockAccount()
    {
        var (email, password) = await fixture.CreateLoginAccountAsync();
        using var client = fixture.Client();
        for (var attempt = 0; attempt < 5; attempt++)
            using (await fixture.LoginAsync(client, email, "Wrong-fictional-password")) { }
        using var locked = await fixture.LoginAsync(client, email, password);
        Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
    }
}
