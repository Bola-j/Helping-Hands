using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore
{
    public Task<string?> GetEmailAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);

    public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(false);

    public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken) => Task.CompletedTask;
}
