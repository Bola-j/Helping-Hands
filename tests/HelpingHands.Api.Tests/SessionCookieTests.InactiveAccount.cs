using System.Net;
using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class SessionCookieTests
{
    [Fact]
    public async Task InactiveAccountCannotLogIn()
    {
        var (email, password) = await fixture.CreateLoginAccountAsync();
        using var scope = fixture.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await users.FindByEmailAsync(email);
        user!.AccountStatus = "Disabled";
        Assert.True((await users.UpdateAsync(user)).Succeeded);
        using var client = fixture.Client();
        using var login = await fixture.LoginAsync(client, email, password);
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }
}
