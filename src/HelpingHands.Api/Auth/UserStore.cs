using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore(HelpingHandsDbContext database) : IUserPasswordStore<AppUser>, IUserEmailStore<AppUser>, IUserRoleStore<AppUser>, IUserSecurityStampStore<AppUser>, IUserLockoutStore<AppUser>
{
    public void Dispose() { }

    public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());

    public Task<string?> GetUserNameAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.UserName);
}
