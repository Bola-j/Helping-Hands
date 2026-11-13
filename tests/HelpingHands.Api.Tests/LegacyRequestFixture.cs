using System.Text.Json;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Tests;

internal static class LegacyRequestFixture
{
    public static async Task<JsonElement> CreateAsync(string connectionVariable, Guid beneficiaryId)
    {
        var options = new DbContextOptionsBuilder<HelpingHandsDbContext>().UseNpgsql(Environment.GetEnvironmentVariable(connectionVariable)).Options;
        await using var db = new HelpingHandsDbContext(options);
        var beneficiary = await db.Beneficiaries.SingleAsync(x => x.Id == beneficiaryId);
        var request = new AssistanceRequest { BeneficiaryId = beneficiaryId, SubmittedByUserId = beneficiary.CreatedByUserId,
            Description = "Fictional need", AuthorityConfirmed = true, SharingAgreed = true, Reference = await db.NextReferenceAsync("REQ") };
        db.AddRange(request, new DecisionHistory { RequestId = request.Id, ActorUserId = beneficiary.CreatedByUserId, Action = "Submitted" });
        await db.SaveChangesAsync();
        return JsonSerializer.SerializeToElement(new { request.Id, request.Reference, request.Description, request.Status, request.CreatedAt, request.Version }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
