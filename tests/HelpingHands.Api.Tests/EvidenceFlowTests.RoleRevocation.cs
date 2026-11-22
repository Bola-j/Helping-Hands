using System.Net;
using HelpingHands.Api.Data;
using HelpingHands.Api.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    [Fact]
    public async Task RevokedRepresentativeRoleConcealsAssignedOutcomeAndEvidence()
    {
        using var admin = fixture.Client();
        using var coordinator = fixture.Client();
        using var representative = fixture.Client();
        using var beneficiary = fixture.Client();
        var representativeEmail = $"revoked-{Guid.NewGuid():N}@example.test";
        await fixture.LoginAsync(admin, fixture.Administrator);
        await fixture.LoginAsync(coordinator, fixture.Coordinator);
        await fixture.RegisterCharityAndLoginAsync(representative, representativeEmail);
        var beneficiaryId = (await fixture.RegisterAndLoginAsync(beneficiary, $"beneficiary-{Guid.NewGuid():N}@example.test"))
            .GetProperty("beneficiaryId").GetGuid();
        var charity = await CreateApprovedCharityAsync(admin, representative);
        var referral = await CreateReferralAsync(coordinator, admin, beneficiary, beneficiaryId, charity);
        var outcome = await EvidenceFlowFixture.PostJsonAsync(representative, $"/api/v1/referrals/{Id(referral)}/outcomes",
            new { ReportedQuantity = 1, Description = "Fictional delivery and receipt" }, HttpStatusCode.Created);
        var attachment = await EvidenceFlowFixture.UploadAsync(representative, Id(outcome), HttpStatusCode.Created);
        using var allowedOutcome = await representative.GetAsync($"/api/v1/outcomes/{Id(outcome)}");
        using var allowedEvidence = await representative.GetAsync($"/api/v1/attachments/{Id(attachment)}/content");
        Assert.Equal(HttpStatusCode.OK, allowedOutcome.StatusCode);
        Assert.Equal(HttpStatusCode.OK, allowedEvidence.StatusCode);
        await RevokeRepresentativeRoleAsync(representativeEmail);
        await AssertRevokedReadsAsync(representative, Id(referral), referral.GetProperty("requestId").GetGuid(), Id(outcome), Id(attachment));
    }

    private async Task RevokeRepresentativeRoleAsync(string email)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var user = await database.Users.SingleAsync(item => item.NormalizedEmail == email.ToUpperInvariant());
        var role = await database.Roles.SingleAsync(item => item.NormalizedName == "RECIPIENTREPRESENTATIVE");
        var link = await database.UserRoles.SingleAsync(item => item.UserId == user.Id && item.RoleId == role.Id);
        database.UserRoles.Remove(link);
        await database.SaveChangesAsync();
    }

    private async Task AssertRevokedReadsAsync(HttpClient representative, Guid referralId, Guid requestId, Guid outcomeId, Guid attachmentId)
    {
        var storage = (EvidenceStorageRecordingProxy)(object)fixture.Factory.Services.GetRequiredService<IEvidenceStorage>();
        var readsBefore = storage.OpenReadCalls;
        using var referral = await representative.GetAsync($"/api/v1/referrals/{referralId}");
        using var outcome = await representative.GetAsync($"/api/v1/outcomes/{outcomeId}");
        using var progress = await representative.GetAsync($"/api/v1/requests/{requestId}/progress");
        using var evidence = await representative.GetAsync($"/api/v1/attachments/{attachmentId}/content");
        Assert.Equal(HttpStatusCode.NotFound, referral.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, outcome.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, progress.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, evidence.StatusCode);
        Assert.Equal(readsBefore, storage.OpenReadCalls);
    }
}
