using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore
{
    public Task SetSecurityStampAsync(AppUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.SecurityStamp);

    public async Task AddToRoleAsync(AppUser user, string role, CancellationToken cancellationToken)
    {
        var roleRecord = await database.Roles.SingleAsync(
            item => item.NormalizedName == role, cancellationToken);
        database.UserRoles.Add(new AppUserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = roleRecord.Id,
            GrantedAt = DateTimeOffset.UtcNow
        });
    }

}
