using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class SessionCookieFixture
{
    public async Task<(string Email, string Password)> CreateLoginAccountAsync()
    {
        var email = $"session-{Guid.NewGuid():N}@example.test";
        var password = $"{Guid.NewGuid():N}Aa1!";
        using var scope = Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var result = await users.CreateAsync(new AppUser { UserName = email, Email = email, DisplayName = "Fictional test account" }, password);
        Assert.True(result.Succeeded);
        return (email, password);
    }
}
