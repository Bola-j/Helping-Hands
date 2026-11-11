using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore
{
    public Task<string?> GetPasswordHashAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash != null);

    public Task SetEmailAsync(AppUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }
}
