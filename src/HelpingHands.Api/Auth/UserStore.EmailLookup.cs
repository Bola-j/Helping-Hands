using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore
{
    public Task<AppUser?> FindByEmailAsync(string email, CancellationToken cancellationToken) => database.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == email, cancellationToken);

    public Task<string?> GetNormalizedEmailAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(AppUser user, string? email, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = email;
        return Task.CompletedTask;
    }
}
