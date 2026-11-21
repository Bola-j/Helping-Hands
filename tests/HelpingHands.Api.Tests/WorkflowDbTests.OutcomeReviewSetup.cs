using System.Net;
using System.Text.Json;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<JsonElement> CreateOutcomeReviewReferralAsync(HttpClient applicant, HttpClient admin,
        HttpClient coordinator, HttpClient beneficiary)
    {
        var (charity, accepted) = await WorkflowDbFixture.AcceptedNeedAsync(applicant, admin, coordinator, beneficiary);
        return await WorkflowDbFixture.CreateReferralAsync(fixture, coordinator, charity, accepted);
    }

    private async Task<(Guid Id, Guid Version)> CreateSubmittedOutcomeAsync(HttpClient representative, JsonElement referral, decimal quantity)
    {
        using var created = await WorkflowDbFixture.PostAsync(representative, $"/api/v1/referrals/{referral.GetProperty("id").GetGuid()}/outcomes",
            new { ReportedQuantity = quantity, Description = "Fictional reported delivery" });
        var outcome = await WorkflowDbFixture.JsonAsync(created, HttpStatusCode.Created);
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var entity = await db.Outcomes.SingleAsync(item => item.Id == outcome.GetProperty("id").GetGuid());
        entity.Status = "Submitted";
        entity.SubmittedAt = DateTimeOffset.UtcNow;
        db.EvidenceAttachments.Add(new EvidenceAttachment
        {
            OutcomeId = entity.Id, UploadedByUserId = fixture.UserIds["RecipientRepresentative"],
            StorageKey = $"fictional/{Guid.NewGuid():N}.pdf", OriginalName = "fictional.pdf", MediaType = "application/pdf", SizeBytes = 1
        });
        await db.SaveChangesAsync();
        return (entity.Id, entity.Version);
    }

    private static async Task ReviewOutcomeAsync(HttpClient coordinator, Guid id, string decision, Guid version, HttpStatusCode expected)
    {
        var input = new { Decision = decision, Reason = (string?)null, Version = version };
        using var response = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/outcomes/{id}/reviews", input);
        if (expected != HttpStatusCode.OK)
        {
            Assert.Equal(expected, response.StatusCode);
            return;
        }
        var result = await WorkflowDbFixture.JsonAsync(response, expected);
        Assert.Equal(decision, result.GetProperty("status").GetString());
    }
}
