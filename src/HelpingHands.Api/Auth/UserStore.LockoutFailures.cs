using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public sealed partial class UserStore
{
    public Task<int> IncrementAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(++user.AccessFailedCount);

    public Task ResetAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(AppUser user, CancellationToken cancellationToken) => Task.FromResult(user.AccessFailedCount);
}
