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

    public static async Task<JsonElement> CreateReferralAsync(WorkflowDbFixture fixture, HttpClient coordinator, JsonElement charity, JsonElement accepted)
    {
        var setup = await WorkflowDbFixture.PrepareLegacyReferralAsync(fixture, coordinator, charity, accepted);
        return await WorkflowDbFixture.PostMappedReferralAsync(coordinator, charity, setup);
    }

    public async Task<(int Count, decimal Quantity)> ReferralTotalsAsync(Guid id)
    {
        using var scope = Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var referrals = database.Referrals.AsNoTracking().Where(item => item.RequestId == id);
        return (await referrals.CountAsync(), await referrals.SumAsync(item => (decimal?)item.AssignedQuantity) ?? 0);
    }
}
