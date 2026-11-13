using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task BeneficiarySignup_RollsBackAccountWhenProfileInsertFails()
    {
        var email = $"rollback-{Guid.NewGuid():N}@example.test";
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHands.Api.Data.HelpingHandsDbContext>();
        await RegistrationFailureFixture.InstallAsync(database, "Beneficiary");
        try
        {
            using var client = fixture.Client();
            using var response = await WorkflowDbFixture.RegisterBeneficiaryAsync(client, "Fictional beneficiary", email, $"{Guid.NewGuid():N}Aa1!");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.False(await database.Users.AnyAsync(user => user.Email == email));
        }
        finally { await RegistrationFailureFixture.RemoveAsync(database, "Beneficiary"); }
    }
}
