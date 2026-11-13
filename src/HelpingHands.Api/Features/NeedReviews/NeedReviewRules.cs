using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static bool InvalidNeedDecision(NeedDecisionInput input) =>
        input.Decision is not ("Accepted" or "Rejected" or "Clarification") || input.Reason?.Length > 2000 || input.ServiceSubtype?.Length > 100 ||
        (input.Decision == "Rejected" && Blank(input.Reason, 2000)) ||
        (input.Decision is "Accepted" or "Rejected" && input.Category is not ("Goods" or "Money" or "OrganizationalReferral")) ||
        (input.Category is not null && input.Category is not ("Goods" or "Money" or "OrganizationalReferral"));

    private static void RollupNeeds(AssistanceRequest request, IReadOnlyCollection<RequestNeed> needs)
    {
        if (needs.Any(x => x.Status == "Clarification")) request.Status = "Clarification";
        else if (needs.Any(x => x.Status is not ("Accepted" or "Rejected"))) request.Status = "InReview";
        else if (needs.All(x => x.Status == "Accepted")) request.Status = "FullyAccepted";
        else if (needs.All(x => x.Status == "Rejected")) request.Status = "FullyRejected";
        else request.Status = "PartiallyAccepted";
        request.Version = Guid.NewGuid();
    }

    private static void RecordNeedDecision(HelpingHandsDbContext db, RequestNeed need, NeedDecisionInput input, Guid actor)
    {
        need.Status = input.Decision;
        need.Category = input.Category ?? need.Category;
        need.ServiceSubtype = input.ServiceSubtype?.Trim();
        db.NeedReviewHistory.Add(new NeedReviewHistory { NeedId = need.Id, ActorUserId = actor, Action = input.Decision,
            Reason = input.Reason?.Trim(), Category = need.Category, ServiceSubtype = need.ServiceSubtype });
    }
}
