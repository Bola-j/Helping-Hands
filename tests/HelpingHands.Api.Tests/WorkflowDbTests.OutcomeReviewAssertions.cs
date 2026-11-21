using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task AssertOutcomeReviewAuditAsync(Guid outcomeId, string reviewerRole, string decision,
        string? reason, DateTimeOffset before, DateTimeOffset after)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var review = await db.OutcomeReviews.SingleAsync(item => item.OutcomeId == outcomeId);
        Assert.Equal(fixture.UserIds[reviewerRole], review.ActorUserId);
        Assert.Equal(decision, review.Decision);
        Assert.Equal(reason, review.Reason);
        Assert.InRange(review.ReviewedAt, before, after);
        Assert.Equal(1, await db.OutcomeReviews.CountAsync(item => item.OutcomeId == outcomeId));
        var history = await db.DecisionHistory.SingleAsync(item => item.Action == $"Outcome{decision}" && item.ReferralId == db.Outcomes.Where(outcome => outcome.Id == outcomeId).Select(outcome => outcome.ReferralId).Single());
        Assert.Equal(review.ActorUserId, history.ActorUserId);
        Assert.Equal(reason, history.Reason);
        Assert.InRange(history.OccurredAt, before, after);
    }

    private async Task AssertOutcomeFlowStateAsync(Guid outcomeId, Guid referralId, Guid requestId,
        string outcomeStatus, string referralStatus, string? requestStatus, decimal acceptedQuantity)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        Assert.Equal(outcomeStatus, (await db.Outcomes.SingleAsync(item => item.Id == outcomeId)).Status);
        Assert.Equal(referralStatus, (await db.Referrals.SingleAsync(item => item.Id == referralId)).Status);
        if (requestStatus is not null)
            Assert.Equal(requestStatus, (await db.Requests.SingleAsync(item => item.Id == requestId)).Status);
        Assert.Equal(acceptedQuantity, await db.Outcomes.Where(item => item.ReferralId == referralId && item.Status == "Accepted").SumAsync(item => item.ReportedQuantity));
    }

    private async Task AssertRejectedExcessOutcomeAsync(Guid acceptedId, Guid rejectedId, Guid referralId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        Assert.Equal("Accepted", (await db.Outcomes.SingleAsync(item => item.Id == acceptedId)).Status);
        Assert.Equal("Submitted", (await db.Outcomes.SingleAsync(item => item.Id == rejectedId)).Status);
        Assert.Equal(0.6m, await db.Outcomes.Where(item => item.ReferralId == referralId && item.Status == "Accepted").SumAsync(item => item.ReportedQuantity));
        var referral = await db.Referrals.SingleAsync(item => item.Id == referralId);
        Assert.Equal("PartiallyFulfilled", referral.Status);
        Assert.Equal("Open", (await db.Requests.SingleAsync(item => item.Id == referral.RequestId)).Status);
        Assert.Equal(1, await db.OutcomeReviews.CountAsync(item => db.Outcomes.Any(outcome => outcome.Id == item.OutcomeId && outcome.ReferralId == referralId)));
    }
}
