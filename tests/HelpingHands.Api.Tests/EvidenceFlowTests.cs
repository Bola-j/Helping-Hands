using System.Net;
using System.Text.Json;

namespace HelpingHands.Api.Tests;

[Trait("Category", "PostgresIntegration")]
public sealed partial class EvidenceFlowTests(EvidenceFlowFixture fixture) : IClassFixture<EvidenceFlowFixture>
{
    private readonly EvidenceFlowFixture fixture = fixture;

    [Fact]
    public async Task FictionalCharityToOutcome_UsesPrivateEvidenceAndRetainsHistory()
    {
        using var admin = fixture.Client();
        using var coordinator = fixture.Client();
        using var representative = fixture.Client();
        using var unassignedRepresentative = fixture.Client();
        using var beneficiary = fixture.Client();
        using var outsider = fixture.Client();
        var beneficiaryId = await OpenSessionsAsync(admin, coordinator, representative, beneficiary, outsider);
        await fixture.RegisterCharityAndLoginAsync(unassignedRepresentative, $"unassigned-{Guid.NewGuid():N}@example.test");
        var charity = await CreateApprovedCharityAsync(admin, representative);
        var referral = await CreateReferralAsync(coordinator, admin, beneficiary, beneficiaryId, charity);
        var result = await SubmitEvidenceAsync(representative, unassignedRepresentative, outsider, referral);
        await AssertOrphanReconciliationAsync(result.OutcomeId);
        await AssertCleanupAfterSubmissionAsync(representative, result.OutcomeId);
        await AcceptAndCheckHistoryAsync(coordinator, result.OutcomeId, result.Version, referral);
    }

    private static Guid Id(JsonElement value) => value.GetProperty("id").GetGuid();
    private static Guid Version(JsonElement value) => value.GetProperty("version").GetGuid();
}
