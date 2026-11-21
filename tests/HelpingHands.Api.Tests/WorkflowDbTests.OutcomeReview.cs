using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Theory]
    [InlineData("Clarification", "Coordinator")]
    [InlineData("Unresolved", "Administrator")]
    public async Task OutcomeReviewRequiresCoordinatorAndPersistsAudit(string decision, string reviewerRole)
    {
        using var reviewer = await fixture.LoginAsync(reviewerRole);
        using var outsider = await fixture.LoginAsync("Unrelated");
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var referral = await CreateOutcomeReviewReferralAsync(applicant, admin, coordinator, beneficiary);
        var (id, version) = await CreateSubmittedOutcomeAsync(applicant, referral, 1m);
        var input = new { Decision = decision, Reason = "Fictional review reason", Version = version };
        Assert.Equal(HttpStatusCode.Forbidden, (await WorkflowDbFixture.PostAsync(outsider, $"/api/v1/outcomes/{id}/reviews", input)).StatusCode);
        var before = DateTimeOffset.UtcNow;
        using var reviewed = await WorkflowDbFixture.PostAsync(reviewer, $"/api/v1/outcomes/{id}/reviews", input);
        var payload = await WorkflowDbFixture.JsonAsync(reviewed, HttpStatusCode.OK);
        Assert.Equal(decision, payload.GetProperty("status").GetString());
        await AssertOutcomeReviewAuditAsync(id, reviewerRole, decision, input.Reason, before, DateTimeOffset.UtcNow);
        await AssertOutcomeFlowStateAsync(id, referral.GetProperty("id").GetGuid(), referral.GetProperty("requestId").GetGuid(), decision, "Assigned", "Open", 0m);
    }

    [Fact]
    public async Task AcceptedOutcomeIsCountedOnceAndRejectsStaleReplay()
    {
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var referral = await CreateOutcomeReviewReferralAsync(applicant, admin, coordinator, beneficiary);
        var (id, version) = await CreateSubmittedOutcomeAsync(applicant, referral, 1m);
        var before = DateTimeOffset.UtcNow;
        await ReviewOutcomeAsync(coordinator, id, "Accepted", version, HttpStatusCode.OK);
        var after = DateTimeOffset.UtcNow;
        Assert.Equal(HttpStatusCode.Conflict, (await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/outcomes/{id}/reviews", new { Decision = "Accepted", Version = version })).StatusCode);
        await AssertOutcomeReviewAuditAsync(id, "Coordinator", "Accepted", null, before, after);
        // Request closure remains P1-55; this ticket verifies the review and referral result only.
        await AssertOutcomeFlowStateAsync(id, referral.GetProperty("id").GetGuid(), referral.GetProperty("requestId").GetGuid(), "Accepted", "Completed", null, 1m);
    }

    [Fact]
    public async Task AcceptedOutcomeCannotExceedReferralQuantity()
    {
        using var applicant = await fixture.LoginAsync("RecipientRepresentative");
        using var admin = await fixture.LoginAsync("Administrator");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        var referral = await CreateOutcomeReviewReferralAsync(applicant, admin, coordinator, beneficiary);
        var (firstId, firstVersion) = await CreateSubmittedOutcomeAsync(applicant, referral, 0.6m);
        var (secondId, secondVersion) = await CreateSubmittedOutcomeAsync(applicant, referral, 0.6m);
        await ReviewOutcomeAsync(coordinator, firstId, "Accepted", firstVersion, HttpStatusCode.OK);
        await ReviewOutcomeAsync(coordinator, secondId, "Accepted", secondVersion, HttpStatusCode.Conflict);
        await AssertRejectedExcessOutcomeAsync(firstId, secondId, referral.GetProperty("id").GetGuid());
    }
}
