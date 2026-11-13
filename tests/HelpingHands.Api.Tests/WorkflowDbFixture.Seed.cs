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
        foreach (var role in new[] { "Administrator", "Coordinator", "Requester", "Beneficiary", "RecipientRepresentative", "Unrelated" })
            await CreateUserAsync(users, role);
        await CreateUserAsync(users, "RecipientRepresentativeOther", "RecipientRepresentative");
    }

    private async Task CreateUserAsync(UserManager<AppUser> users, string role, string? assignedRole = null)
    {
        var email = $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}@example.test";
        var user = new AppUser { UserName = email, Email = email, DisplayName = "Fictional user" };
        Assert.True((await users.CreateAsync(user, Password)).Succeeded);
        if (role != "Unrelated") Assert.True((await users.AddToRoleAsync(user, assignedRole ?? role)).Succeeded);
        Emails.Add(role, email);
        UserIds.Add(role, user.Id);
    }
}
