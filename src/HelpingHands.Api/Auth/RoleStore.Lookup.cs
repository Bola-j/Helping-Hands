using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class RoleStore
{
    public Task<AppRole?> FindByIdAsync(string id, CancellationToken cancellationToken) => Guid.TryParse(id, out var parsedId) ? database.Roles.SingleOrDefaultAsync(x => x.Id == parsedId, cancellationToken) : Task.FromResult<AppRole?>(null);

    public Task<AppRole?> FindByNameAsync(string name, CancellationToken cancellationToken) => database.Roles.SingleOrDefaultAsync(x => x.NormalizedName == name, cancellationToken);
}
