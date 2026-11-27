using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<ProgressSeed> CreateProgressAssignmentsAsync(HelpingHandsDbContext db,
        AssistanceRequest request, IReadOnlyList<RequestNeed> needs)
    {
        var charities = await CreateProgressCharitiesAsync(db);
        var refs = new[] { await db.NextReferenceAsync("REF"), await db.NextReferenceAsync("REF") };
        var referrals = charities.Select((item, index) => new Referral { RequestId = request.Id, NeedId = needs[index].Id,
            RecipientId = item.Id, AssignedQuantity = 1m, ScopeDescription = $"Need {index + 1}",
            CreatedByUserId = fixture.UserIds["Coordinator"], Reference = refs[index] }).ToArray();
        db.Referrals.AddRange(referrals);
        db.DecisionHistory.AddRange(referrals.Select(item => new DecisionHistory { ReferralId = item.Id,
            ActorUserId = fixture.UserIds["Coordinator"], Action = "SplitAllocated", Reason = "Fictional allocation" }));
        var outcomeIds = await CreateProgressOutcomesAsync(db, referrals[0].Id);
        await db.SaveChangesAsync();
        return new ProgressSeed(request.Id, needs[0].Id, needs[1].Id, referrals[0].Id, referrals[1].Id,
            outcomeIds.Accepted, outcomeIds.Clarification, outcomeIds.Unresolved, outcomeIds.EvidenceAttachment);
    }

    private async Task<Recipient[]> CreateProgressCharitiesAsync(HelpingHandsDbContext db)
    {
        var ids = new[] { fixture.UserIds["RecipientRepresentative"], fixture.UserIds["RecipientRepresentativeOther"] };
        var recipients = new Recipient[2];
        for (var index = 0; index < recipients.Length; index++)
        {
            recipients[index] = new Recipient { Reference = await db.NextReferenceAsync("REC"), DisplayName = $"Fictional charity {index + 1}", ApprovalStatus = "Approved", Active = true };
            db.RecipientMembers.Add(new RecipientMember { RecipientId = recipients[index].Id, UserId = ids[index], IsActive = true });
        }
        db.Recipients.AddRange(recipients);
        await db.SaveChangesAsync();
        return recipients;
    }

    private async Task<(Guid Accepted, Guid Clarification, Guid Unresolved, Guid EvidenceAttachment)> CreateProgressOutcomesAsync(
        HelpingHandsDbContext db, Guid referralId)
    {
        var reviews = new[] { ("Accepted", "Internal acceptance note"),
            ("Clarification", "Please clarify the receipt"), ("Unresolved", "Internal unresolved note") };
        var outcomes = new List<Outcome>();
        foreach (var (decision, reason) in reviews)
        {
            var outcome = new Outcome { ReferralId = referralId, ReportedByUserId = fixture.UserIds["RecipientRepresentative"],
                ReportedQuantity = 1m, Description = $"Fictional {decision.ToLowerInvariant()} report",
                Status = decision, Reference = await db.NextReferenceAsync("OUT") };
            outcomes.Add(outcome);
            db.OutcomeReviews.Add(new OutcomeReview { OutcomeId = outcome.Id, ActorUserId = fixture.UserIds["Coordinator"],
                Decision = decision, Reason = reason });
        }
        db.Outcomes.AddRange(outcomes);
        var evidence = new EvidenceAttachment
        {
            OutcomeId = outcomes[0].Id, UploadedByUserId = fixture.UserIds["RecipientRepresentative"],
            StorageKey = $"fictional/{Guid.NewGuid():N}.pdf", OriginalName = "fictional receipt.pdf",
            MediaType = "application/pdf", SizeBytes = 4096,
        };
        db.EvidenceAttachments.Add(evidence);
        return (outcomes[0].Id, outcomes[1].Id, outcomes[2].Id, evidence.Id);
    }
}
