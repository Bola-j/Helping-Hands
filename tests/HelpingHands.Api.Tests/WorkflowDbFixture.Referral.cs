using System.Text.Json;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbFixture
{
    public static async Task<(JsonElement Recipient, JsonElement Request)> AcceptedNeedAsync(
        HttpClient applicant, HttpClient admin, HttpClient coordinator)
    {
        var charity = await RecipientAsync(applicant);
        await ApproveAsync(admin, charity);
        var beneficiary = await BeneficiaryAsync(coordinator);
        var request = await RequestAsync(coordinator, beneficiary.GetProperty("id").GetGuid());
        return (charity, await AcceptAsync(coordinator, request));
    }

    public async Task<(int Count, decimal Quantity)> ReferralTotalsAsync(Guid id)
    {
        using var scope = Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var referrals = database.Referrals.AsNoTracking().Where(item => item.RequestId == id);
        return (await referrals.CountAsync(), await referrals.SumAsync(item => (decimal?)item.AssignedQuantity) ?? 0);
    }
}
