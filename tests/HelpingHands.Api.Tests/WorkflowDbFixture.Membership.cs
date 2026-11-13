using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    public async Task SetMembershipActiveAsync(Guid recipientId, string role, bool active)
    {
        using var scope = Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        await database.RecipientMembers.Where(item => item.RecipientId == recipientId && item.UserId == UserIds[role])
            .ExecuteUpdateAsync(updates => updates.SetProperty(item => item.IsActive, active));
    }
}
