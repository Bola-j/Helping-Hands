namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    public sealed record NeedDecisionInput(string Decision, string? Category, string? ServiceSubtype, string? Reason, Guid Version);
    public sealed record NeedClarificationInput(string Explanation, Guid Version);

    private static void MapNeedReviewRoutes(this RouteGroupBuilder api)
    {
        api.MapPost("/requests/{requestId:guid}/needs/{needId:guid}/decisions", DecideNeed).RequireAuthorization();
        api.MapPost("/requests/{requestId:guid}/needs/{needId:guid}/clarifications", ClarifyNeed).RequireAuthorization();
        api.MapGet("/requests/{requestId:guid}/needs/{needId:guid}/history", NeedHistory).RequireAuthorization();
    }
}
