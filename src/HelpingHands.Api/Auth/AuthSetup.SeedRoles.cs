using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;

namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static async Task EnsureRoles(RoleManager<AppRole> roles)
    {
        foreach (var name in new[] { "Administrator", "Coordinator", "Requester", "RecipientRepresentative" })
        {
            if (!await roles.RoleExistsAsync(name))
                Ensure(await roles.CreateAsync(new AppRole { Id = Guid.NewGuid(), Name = name }));
        }
    }
}
