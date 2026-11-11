using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class RoleStore
{
    public async Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancellationToken)
    {
        database.Roles.Add(role);
        await database.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancellationToken)
    {
        role.ConcurrencyStamp = Guid.NewGuid().ToString();
        try
        {
            database.Roles.Update(role);
            await database.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(new IdentityErrorDescriber().ConcurrencyFailure());
        }
    }

    public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken)
    {
        database.Roles.Remove(role);
        await database.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }
}
