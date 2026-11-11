using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class RoleStore(HelpingHandsDbContext database) : IRoleStore<AppRole>
{
    public void Dispose() { }

    public Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancellationToken) => Task.FromResult(role.Id.ToString());

    public Task<string?> GetRoleNameAsync(AppRole role, CancellationToken cancellationToken) => Task.FromResult<string?>(role.Name);
}
