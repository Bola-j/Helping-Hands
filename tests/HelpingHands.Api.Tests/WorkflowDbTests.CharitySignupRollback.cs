using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CharitySignup_RollsBackAccountWhenMembershipInsertFails()
    {
        var email = $"rollback-{Guid.NewGuid():N}@example.test";
        var organization = $"Fictional rollback charity {Guid.NewGuid():N}";
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHands.Api.Data.HelpingHandsDbContext>();
        await RegistrationFailureFixture.InstallAsync(database, "RecipientMember");
        try
        {
            using var response = await WorkflowDbFixture.RegisterCharityAsync(fixture.Client(), "Fictional representative", email, $"{Guid.NewGuid():N}Aa1!", organization, "Fictional contact");
            Assert.True(response.StatusCode == HttpStatusCode.InternalServerError, await response.Content.ReadAsStringAsync());
            Assert.False(await database.Users.AnyAsync(user => user.Email == email));
            Assert.False(await database.Recipients.AnyAsync(item => item.DisplayName == organization));
        }
        finally { await RegistrationFailureFixture.RemoveAsync(database, "RecipientMember"); }
    }
}
