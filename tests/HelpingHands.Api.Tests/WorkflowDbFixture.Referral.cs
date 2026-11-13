using System.Net;
using System.Text.Json;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    public static async Task<(JsonElement Recipient, JsonElement Request)> AcceptedNeedAsync(
        HttpClient applicant, HttpClient admin, HttpClient coordinator, HttpClient beneficiary)
    {
        var charity = await RecipientAsync(applicant);
        await ApproveAsync(admin, charity);
        var profile = await BeneficiaryAsync(beneficiary);
        var request = await RequestAsync(beneficiary, profile.GetProperty("id").GetGuid());
        return (charity, await AcceptAsync(coordinator, request));
    }

    public static async Task<JsonElement> CreateReferralAsync(HttpClient coordinator, JsonElement charity, JsonElement accepted)
    {
        var requestId = accepted.GetProperty("id").GetGuid();
        var response = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{requestId}/referrals", new
        {
            RecipientId = charity.GetProperty("id").GetGuid(),
            AssignedQuantity = 1m,
            ScopeDescription = "Fictional assignment",
            Version = accepted.GetProperty("version").GetGuid()
        });
        return await WorkflowDbFixture.JsonAsync(response, HttpStatusCode.Created);
    }

    public async Task<(int Count, decimal Quantity)> ReferralTotalsAsync(Guid id)
    {
        using var scope = Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var referrals = database.Referrals.AsNoTracking().Where(item => item.RequestId == id);
        return (await referrals.CountAsync(), await referrals.SumAsync(item => (decimal?)item.AssignedQuantity) ?? 0);
    }
}
