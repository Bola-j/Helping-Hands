using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static bool InvalidOutcomeReview(ReviewInput input) =>
        input.Decision is not ("Accepted" or "Clarification" or "Unresolved") || input.Reason?.Length > 2000;

    private static bool CannotReviewOutcome(Outcome outcome, AssistanceRequest request, ReviewInput input) =>
        outcome.Version != input.Version || outcome.Status != "Submitted" || request.Status != "Open";
}
