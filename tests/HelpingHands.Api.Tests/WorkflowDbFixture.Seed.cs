using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    private async Task SeedUsersAsync(IServiceProvider provider)
    {
        var roles = provider.GetRequiredService<RoleManager<AppRole>>();
        var users = provider.GetRequiredService<UserManager<AppUser>>();
        foreach (var role in new[] { "Administrator", "Coordinator", "Requester", "Beneficiary", "RecipientRepresentative" })
            if (!await roles.RoleExistsAsync(role)) Assert.True((await roles.CreateAsync(new AppRole { Name = role })).Succeeded);
        foreach (var role in new[] { "Administrator", "Coordinator", "Requester", "Unrelated" })
            await CreateUserAsync(users, role);
    }

    private async Task CreateUserAsync(UserManager<AppUser> users, string role)
    {
        var email = $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}@example.test";
        var user = new AppUser { UserName = email, Email = email, DisplayName = "Fictional user" };
        Assert.True((await users.CreateAsync(user, Password)).Succeeded);
        if (role != "Unrelated") Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        Emails.Add(role, email);
    }
}
