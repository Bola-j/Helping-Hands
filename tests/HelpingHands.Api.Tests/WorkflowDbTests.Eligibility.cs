using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task PendingAndRejectedCharities_CannotReceiveReferrals()
    {
        using var applicant = await fixture.LoginAsync("Requester");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        var charity = await WorkflowDbFixture.RecipientAsync(applicant);
        var accepted = await WorkflowDbFixture.AcceptedRequestAsync(coordinator);
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
    }
}
