using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task AssertPendingCharitySignupAsync(HttpClient client, HttpResponseMessage response, string email, string password)
    {
        Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        WorkflowDbFixture.AssertNoAuthenticationCookie(response);
        var charity = (await response.Content.ReadFromJsonAsync<JsonElement>())!;
        Assert.Equal("Pending", charity.GetProperty("approvalStatus").GetString());
        Assert.False(charity.GetProperty("active").GetBoolean());
        Assert.StartsWith("REC-", charity.GetProperty("reference").GetString());
        Assert.DoesNotContain(password, charity.GetRawText());
        using var login = await WorkflowDbFixture.LoginRegisteredAsync(client, email, password);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        await AssertCharityRecordsAsync(charity);
    }

    private async Task AssertCharityRecordsAsync(JsonElement charity)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var recipientId = charity.GetProperty("recipientId").GetGuid();
        var member = await database.RecipientMembers.SingleAsync(item => item.RecipientId == recipientId);
        Assert.Equal(charity.GetProperty("accountId").GetGuid(), member.UserId);
        Assert.Equal("Pending", (await database.Recipients.SingleAsync(item => item.Id == recipientId)).ApprovalStatus);
        Assert.Null((await database.Charities.SingleAsync(item => item.RecipientId == recipientId)).RegistrationInformation);
        Assert.Equal("RecipientRepresentative", await RoleNameAsync(database, member.UserId));
    }

    private static Task<string> RoleNameAsync(HelpingHandsDbContext database, Guid userId) =>
        database.UserRoles.Where(role => role.UserId == userId).Join(database.Roles, link => link.RoleId, role => role.Id, (_, role) => role.Name).SingleAsync();
}
