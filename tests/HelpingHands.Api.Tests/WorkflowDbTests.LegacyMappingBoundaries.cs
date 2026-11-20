using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task LegacyReferral_RequiresExplicitMappingWithoutChangingExistingTotals()
    {
        using var representative = await fixture.LoginAsync("RecipientRepresentative");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var (charity, request) = await WorkflowDbFixture.AcceptedNeedAsync(representative, admin, coordinator, beneficiary);
        var requestId = request.GetProperty("id").GetGuid();
        using var response = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{requestId}/referrals", new
        {
            RecipientId = charity.GetProperty("id").GetGuid(), AssignedQuantity = 1m,
            ScopeDescription = "Fictional unmapped referral", Version = request.GetProperty("version").GetGuid()
        });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal((0, 0m), await fixture.ReferralTotalsAsync(requestId));
    }

    [Fact]
    public async Task LegacyStandardMapping_RejectsMultiNeedRequests()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (_, _, _, standard) = await PublishedCatalogAsync(db);
        request.Status = "Accepted";
        await db.SaveChangesAsync();
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var response = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{request.Id}/legacy-standard", new
        { StandardVersionId = standard.Id, Version = request.Version });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
