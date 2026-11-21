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
    public async Task ReferralNeedMustBelongToTheSameRequest()
    {
        var seed = await SeedRequestProgressAsync();
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (_, otherRequest) = await NeedRequestAsync(scope);
        var foreignNeed = (await CreateProgressNeedsAsync(db, otherRequest))[0];
        var existing = await db.Referrals.FirstAsync(item => item.RequestId == seed.RequestId);
        var invalid = new Referral { RequestId = seed.RequestId, NeedId = foreignNeed.Id,
            RecipientId = existing.RecipientId, AssignedQuantity = 1m, ScopeDescription = "Invalid cross-request link",
            CreatedByUserId = fixture.UserIds["Coordinator"], Reference = await db.NextReferenceAsync("REF") };
        db.Referrals.Add(invalid);
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task RequestProgress_IsScopedToBeneficiaryRepresentativeAndCoordinator()
    {
        var seed = await SeedRequestProgressAsync();
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        using var representative = await fixture.LoginAsync("RecipientRepresentative");
        using var unrelated = await fixture.LoginAsync("RecipientRepresentativeOther");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        var own = await RequestProgressAsync(beneficiary, seed.RequestId, HttpStatusCode.OK);
        Assert.Equal(2, own.GetProperty("needs").GetArrayLength());
        var rep = await RequestProgressAsync(representative, seed.RequestId, HttpStatusCode.OK);
        Assert.Equal(1, rep.GetProperty("needs").GetArrayLength());
        Assert.Equal(JsonValueKind.Null, rep.GetProperty("status").ValueKind);
        Assert.Equal(JsonValueKind.Null, rep.GetProperty("description").ValueKind);
        var repNeed = rep.GetProperty("needs")[0];
        Assert.Equal(JsonValueKind.Null, repNeed.GetProperty("requiredQuantity").ValueKind);
        Assert.Equal(JsonValueKind.Null, repNeed.GetProperty("status").ValueKind);
        Assert.Equal(JsonValueKind.Null, repNeed.GetProperty("remainingQuantity").ValueKind);
        Assert.Equal(1m, repNeed.GetProperty("assignedQuantity").GetDecimal());
        Assert.Equal(1m, repNeed.GetProperty("acceptedQuantity").GetDecimal());
        Assert.Equal(1, repNeed.GetProperty("assignments").GetArrayLength());
        Assert.Equal("Accepted", repNeed.GetProperty("assignments")[0].GetProperty("outcomes")[0].GetProperty("status").GetString());
        Assert.DoesNotContain(rep.GetProperty("history").EnumerateArray(),
            item => item.GetProperty("entityId").GetGuid() == seed.SecondReferralId);
        var otherCharity = await RequestProgressAsync(unrelated, seed.RequestId, HttpStatusCode.OK);
        Assert.Equal(seed.SecondNeedId, otherCharity.GetProperty("needs")[0].GetProperty("needId").GetGuid());
        await RequestProgressAsync(unrelated, await SeedUnassignedProgressRequestAsync(), HttpStatusCode.NotFound);
        var staff = await RequestProgressAsync(coordinator, seed.RequestId, HttpStatusCode.OK);
        Assert.Equal(2, staff.GetProperty("needs").GetArrayLength());
        Assert.Contains(staff.GetProperty("history").EnumerateArray(),
            item => item.GetProperty("actorUserId").ValueKind != JsonValueKind.Null);
    }

    private static async Task<JsonElement> RequestProgressAsync(HttpClient client, Guid id, HttpStatusCode status)
    {
        using var response = await client.GetAsync($"/api/v1/requests/{id}/progress");
        Assert.Equal(status, response.StatusCode);
        return status == HttpStatusCode.OK ? await response.Content.ReadFromJsonAsync<JsonElement>() : default;
    }
}
