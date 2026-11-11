using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore
{
    public async Task RemoveFromRoleAsync(AppUser user, string role, CancellationToken cancellationToken)
    {
        var membership = await (from member in database.UserRoles
                                join record in database.Roles on member.RoleId equals record.Id
                                where member.UserId == user.Id && record.NormalizedName == role
                                select member).SingleOrDefaultAsync(cancellationToken);
        if (membership is not null)
            database.UserRoles.Remove(membership);
    }

    public async Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken cancellationToken) =>
        await (from member in database.UserRoles
               join record in database.Roles on member.RoleId equals record.Id
               where member.UserId == user.Id
               select record.Name!).ToListAsync(cancellationToken);

    public Task<bool> IsInRoleAsync(AppUser user, string role, CancellationToken cancellationToken) =>
        (from member in database.UserRoles
         join record in database.Roles on member.RoleId equals record.Id
         where member.UserId == user.Id && record.NormalizedName == role
         select member).AnyAsync(cancellationToken);

}
