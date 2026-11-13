using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task RegisteredRequester_CanCreateAndReadBeneficiary()
    {
        using var client = fixture.Client();
        var userId = await RegisterAndLoginRequesterAsync(client);
        using var created = await WorkflowDbFixture.PostAsync(client, "/api/v1/beneficiaries", new
        { DisplayName = "Fictional beneficiary", ContactDetails = "Fictional contact", IsFamily = false });
        var beneficiary = await WorkflowDbFixture.JsonAsync(created, HttpStatusCode.Created);
        using var visible = await client.GetAsync($"/api/v1/beneficiaries/{beneficiary.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.OK, visible.StatusCode);
        Assert.NotEqual(Guid.Empty, userId);
    }

    [Fact]
    public async Task RegisteredAccount_CanSubmitCharityApplicationPendingReview()
    {
        using var client = fixture.Client();
        var userId = await RegisterAndLoginRequesterAsync(client);
        using var response = await WorkflowDbFixture.PostAsync(client, "/api/v1/recipients", new
        { DisplayName = "Fictional charity", ContactDetails = "Fictional contact", RegistrationInformation = "Fictional registration" });
        var charity = await WorkflowDbFixture.JsonAsync(response, HttpStatusCode.Created);
        Assert.Equal("Pending", charity.GetProperty("approvalStatus").GetString());
        Assert.False(charity.GetProperty("active").GetBoolean());
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var member = await database.RecipientMembers.SingleAsync(item => item.RecipientId == charity.GetProperty("id").GetGuid());
        Assert.Equal(userId, member.UserId);
    }

    [Theory]
    [InlineData("", "person@example.test", "FictionalPassword123!")]
    [InlineData("Fictional person", "not-an-email", "FictionalPassword123!")]
    [InlineData("Fictional person", "person@example.test", "weak")]
    public async Task InvalidRegistration_IsRejected(string name, string email, string password)
    {
        using var client = fixture.Client();
        using var response = await WorkflowDbFixture.RegisterAsync(client, name, email, password);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
