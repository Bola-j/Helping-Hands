using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static bool InvalidRequestDecision(RequestDecisionInput input) =>
        input.Decision is not ("Accepted" or "Rejected" or "Clarification") ||
        input.Reason?.Length > 2000 ||
        (input.Decision == "Rejected" && Blank(input.Reason, 2000));

    private static bool InvalidAcceptedRequest(RequestDecisionInput input) =>
        input.Category is not ("Goods" or "Money" or "OrganizationalReferral") ||
        input.RequiredQuantity is null or <= 0 || !Precision(input.RequiredQuantity.Value) ||
        Blank(input.Unit, 50) || input.ServiceSubtype?.Length > 100 ||
        (input.Unit == "scope" && input.RequiredQuantity != 1);

    private static void ApplyRequestClassification(AssistanceRequest request, RequestDecisionInput input)
    {
        if (input.Decision != "Accepted") return;
        request.Category = input.Category;
        request.RequiredQuantity = input.RequiredQuantity;
        request.Unit = input.Unit;
        request.ServiceSubtype = input.ServiceSubtype;
    }
}
