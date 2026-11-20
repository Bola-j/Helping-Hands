using System.Net;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task PendingAndRejectedCharities_CannotReceiveReferrals()
    {
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var charity = await WorkflowDbFixture.RecipientAsync(applicant);
        var accepted = await WorkflowDbFixture.AcceptedRequestAsync(beneficiary, coordinator);
        var charityId = charity.GetProperty("id").GetGuid();
        var path = $"/api/v1/requests/{accepted.GetProperty("id").GetGuid()}/referrals";
        var body = new { RecipientId = charityId, AssignedQuantity = 1m, ScopeDescription = "Fictional", Version = accepted.GetProperty("version").GetGuid() };
        using var pending = await WorkflowDbFixture.PostAsync(coordinator, path, body);
        Assert.Equal(HttpStatusCode.Conflict, pending.StatusCode);
        using var rejected = await WorkflowDbFixture.PostAsync(admin, $"/api/v1/recipients/{charityId}/decisions", new
        { Decision = "Rejected", Reason = "Fictional reason", Version = charity.GetProperty("version").GetGuid() });
        Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);
        using var refused = await WorkflowDbFixture.PostAsync(coordinator, path, body);
        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal((0, 0m), await fixture.ReferralTotalsAsync(accepted.GetProperty("id").GetGuid()));
        using var unchanged = await coordinator.GetAsync($"/api/v1/requests/{accepted.GetProperty("id").GetGuid()}");
        var current = await WorkflowDbFixture.JsonAsync(unchanged, HttpStatusCode.OK);
        Assert.Equal(accepted.GetProperty("version").GetGuid(), current.GetProperty("version").GetGuid());
    }

    [Fact]
    public async Task ApprovedInactiveCharity_CannotReceiveReferral_ButActiveCharityCan()
    {
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var (charity, accepted) = await WorkflowDbFixture.AcceptedNeedAsync(applicant, admin, coordinator, beneficiary);
        var setup = await WorkflowDbFixture.PrepareLegacyReferralAsync(fixture, coordinator, charity, accepted);
        var id = charity.GetProperty("id").GetGuid();
        await SetCharityActiveAsync(id, false);
        var requestId = setup.Accepted.GetProperty("id").GetGuid();
        using var denied = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{requestId}/referrals", new
        {
            RecipientId = id,
            AssignedQuantity = 1m,
            ScopeDescription = "Fictional",
            Version = setup.Accepted.GetProperty("version").GetGuid(),
            ServiceVersionId = setup.ServiceVersionId,
            StandardVersionId = setup.StandardVersionId
        });
        Assert.Equal(HttpStatusCode.Conflict, denied.StatusCode);
        Assert.Equal((0, 0m), await fixture.ReferralTotalsAsync(requestId));
        await SetCharityActiveAsync(id, true);
        await WorkflowDbFixture.PostMappedReferralAsync(coordinator, charity, setup);
        Assert.Equal((1, 1m), await fixture.ReferralTotalsAsync(requestId));
    }

    private async Task SetCharityActiveAsync(Guid id, bool active)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var recipient = await db.Recipients.SingleAsync(item => item.Id == id);
        recipient.Active = active;
        await db.SaveChangesAsync();
    }
}
