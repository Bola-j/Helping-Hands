using System.Net;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CompetingReferrals_AllocateAvailableNeedOnce()
    {
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var (charity, accepted) = await WorkflowDbFixture.AcceptedNeedAsync(applicant, admin, coordinator, beneficiary);
        var requestId = accepted.GetProperty("id").GetGuid();
        var path = $"/api/v1/requests/{requestId}/referrals";
        var body = new
        {
            RecipientId = charity.GetProperty("id").GetGuid(),
            AssignedQuantity = 1m,
            ScopeDescription = "Fictional assignment",
            Version = accepted.GetProperty("version").GetGuid()
        };
        var responses = await Task.WhenAll(
            WorkflowDbFixture.PostAsync(coordinator, path, body),
            WorkflowDbFixture.PostAsync(admin, path, body));
        try
        {
            Assert.Equal(new[] { HttpStatusCode.Created, HttpStatusCode.Conflict },
                responses.Select(response => response.StatusCode).OrderBy(status => (int)status));
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }
        var totals = await fixture.ReferralTotalsAsync(requestId);
        Assert.Equal((1, 1m), totals);
    }

    [Fact]
    public async Task RepresentativeRoleWithoutMembershipCannotReadReferralOrOutcome()
    {
        using var representative = await fixture.LoginAsync("RecipientRepresentative");
        using var otherRepresentative = await fixture.LoginAsync("RecipientRepresentativeOther");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var (charity, accepted) = await WorkflowDbFixture.AcceptedNeedAsync(representative, admin, coordinator, beneficiary);
        var referral = await WorkflowDbFixture.CreateReferralAsync(coordinator, charity, accepted);
        var outcome = await WorkflowDbFixture.CreateOutcomeAsync(representative, referral);
        using var hiddenReferral = await otherRepresentative.GetAsync($"/api/v1/referrals/{referral.GetProperty("id").GetGuid()}");
        using var hiddenOutcome = await otherRepresentative.GetAsync($"/api/v1/outcomes/{outcome.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, hiddenReferral.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, hiddenOutcome.StatusCode);
    }
}
