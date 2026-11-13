using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Theory]
    [InlineData("Fictional representative", "bad-email", "FictionalPassword123!", "Fictional charity", "Fictional contact")]
    [InlineData("Fictional representative", "valid@example.test", "weak", "Fictional charity", "Fictional contact")]
    [InlineData("Fictional representative", "valid@example.test", "FictionalPassword123!", "", "Fictional contact")]
    [InlineData("Fictional representative", "valid@example.test", "FictionalPassword123!", "Fictional charity", "")]
    public async Task CharitySignup_RejectsInvalidRequiredFields(string name, string email, string password, string organization, string contact)
    {
        using var client = fixture.Client();
        using var response = await WorkflowDbFixture.RegisterCharityAsync(client, name, email, password, organization, contact);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RemovedSharedRegistrationRoute_IsNotFound()
    {
        using var client = fixture.Client();
        using var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("", "person@example.test", "FictionalPassword123!")]
    [InlineData("Fictional person", "not-an-email", "FictionalPassword123!")]
    [InlineData("Fictional person", "person@example.test", "weak")]
    public async Task InvalidBeneficiarySignup_IsRejected(string name, string email, string password)
    {
        using var client = fixture.Client();
        using var response = await WorkflowDbFixture.RegisterBeneficiaryAsync(client, name, email, password);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHands.Api.Data.HelpingHandsDbContext>();
        Assert.False(await database.Users.AnyAsync(user => user.Email == email));
    }
}
