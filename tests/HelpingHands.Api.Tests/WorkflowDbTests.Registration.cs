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
    public async Task BeneficiarySignup_CreatesLinkedProfileAndLoginSession()
    {
        using var client = fixture.Client();
        var userId = await RegisterAndLoginBeneficiaryAsync(client);
        using var profileResponse = await client.GetAsync("/api/v1/auth/me");
        var profile = await WorkflowDbFixture.JsonAsync(profileResponse, HttpStatusCode.OK);
        Assert.Contains(profile.GetProperty("roles").EnumerateArray(), role => role.GetString() == "Beneficiary");
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var beneficiary = await database.Beneficiaries.SingleAsync(item => item.LinkedUserId == userId);
        Assert.Equal("Fictional requester", beneficiary.DisplayName);
        Assert.Equal("Fictional contact", beneficiary.ContactDetails);
        Assert.True(beneficiary.IsFamily);
        Assert.StartsWith("BEN-", beneficiary.Reference);
        using var visible = await client.GetAsync($"/api/v1/beneficiaries/{beneficiary.Id}");
        Assert.Equal(HttpStatusCode.OK, visible.StatusCode);
    }

    [Fact]
    public async Task CharitySignup_CreatesRepresentativeMembershipAndPendingApplication()
    {
        using var client = fixture.Client();
        var email = $"charity-{Guid.NewGuid():N}@example.test";
        var password = $"{Guid.NewGuid():N}Aa1!";
        using var response = await WorkflowDbFixture.RegisterCharityAsync(client, "Fictional representative", email, password, "Fictional charity", "Fictional contact");
        await AssertPendingCharitySignupAsync(client, response, email, password);
    }

}
