using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CompetingReferrals_AllocateAvailableNeedOnce()
    {
        using var applicant = await fixture.LoginAsync("Requester");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var second = await fixture.LoginAsync("Coordinator");
        var (charity, accepted) = await WorkflowDbFixture.AcceptedNeedAsync(applicant, admin, coordinator);
        var requestId = accepted.GetProperty("id").GetGuid();
        var path = $"/api/v1/requests/{requestId}/referrals";
        var body = new { RecipientId = charity.GetProperty("id").GetGuid(), AssignedQuantity = 1m,
            ScopeDescription = "Fictional assignment", Version = accepted.GetProperty("version").GetGuid() };
        var responses = await Task.WhenAll(WorkflowDbFixture.PostAsync(coordinator, path, body), WorkflowDbFixture.PostAsync(second, path, body));
        try { Assert.Equal(new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }, responses.Select(x => x.StatusCode).OrderBy(x => (int)x)); }
        finally { foreach (var response in responses) response.Dispose(); }
        var totals = await fixture.ReferralTotalsAsync(requestId);
        Assert.Equal((1, 1m), totals);
    }
}
