using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore
{
    public async Task<IList<AppUser>> GetUsersInRoleAsync(string role, CancellationToken cancellationToken) =>
        await (from membership in database.UserRoles
               join record in database.Roles on membership.RoleId equals record.Id
               join user in database.Users on membership.UserId equals user.Id
               where record.NormalizedName == role
               select user).ToListAsync(cancellationToken);

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.LockoutEnd);

    public Task SetLockoutEndDateAsync(AppUser user, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        user.LockoutEnd = date;
        return Task.CompletedTask;
    }
}
