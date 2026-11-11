using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore
{
    public Task<AppUser?> FindByIdAsync(string id, CancellationToken cancellationToken) => Guid.TryParse(id, out var parsedId) ? database.Users.SingleOrDefaultAsync(x => x.Id == parsedId, cancellationToken) : Task.FromResult<AppUser?>(null);

    public Task<AppUser?> FindByNameAsync(string name, CancellationToken cancellationToken) => database.Users.SingleOrDefaultAsync(x => x.NormalizedUserName == name, cancellationToken);

    public Task SetPasswordHashAsync(AppUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }
}
