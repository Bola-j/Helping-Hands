using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore
{
    public Task SetUserNameAsync(AppUser user, string? name, CancellationToken cancellationToken)
    {
        user.UserName = name ?? "";
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(AppUser user, string? name, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = name ?? "";
        return Task.CompletedTask;
    }
}
