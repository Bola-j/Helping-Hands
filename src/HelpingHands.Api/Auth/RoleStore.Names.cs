using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class RoleStore
{
    public Task SetRoleNameAsync(AppRole role, string? name, CancellationToken cancellationToken)
    {
        role.Name = name ?? "";
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancellationToken) => Task.FromResult<string?>(role.NormalizedName);

    public Task SetNormalizedRoleNameAsync(AppRole role, string? name, CancellationToken cancellationToken)
    {
        role.NormalizedName = name ?? "";
        return Task.CompletedTask;
    }
}
