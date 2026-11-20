namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static void MapParticipantRoutes(this RouteGroupBuilder api)
    {
        api.MapPost("/recipients", CreateRecipient).RequireAuthorization();
        api.MapGet("/recipients", ListRecipients).RequireAuthorization();
        api.MapGet("/recipients/{id:guid}", GetRecipient).RequireAuthorization();
        api.MapPost("/recipients/{id:guid}/decisions", DecideRecipient).RequireAuthorization();
        api.MapRecipientServiceRoutes();
        api.MapRecipientCapacityRoutes();
        api.MapPost("/beneficiaries", CreateBeneficiary).RequireAuthorization();
        api.MapGet("/beneficiaries", ListBeneficiaries).RequireAuthorization();
        api.MapGet("/beneficiaries/{id:guid}", GetBeneficiary).RequireAuthorization();
    }

    private static void MapRequestAndReferralRoutes(this RouteGroupBuilder api)
    {
        api.MapPost("/requests", CreateRequest).RequireAuthorization();
        api.MapGet("/requests", ListRequests).RequireAuthorization();
        api.MapGet("/requests/{id:guid}", GetRequest).RequireAuthorization();
        api.MapPut("/requests/{id:guid}", UpdateRequest).RequireAuthorization();
        api.MapPost("/requests/{id:guid}/decisions", DecideRequest).RequireAuthorization();
        api.MapPost("/requests/{id:guid}/legacy-standard", MapLegacyStandard).RequireAuthorization();
        api.MapGet("/requests/{id:guid}/history", RequestHistory).RequireAuthorization();
        api.MapPost("/requests/{id:guid}/referrals", CreateReferral).RequireAuthorization();
        api.MapGet("/referrals", ListReferrals).RequireAuthorization();
        api.MapGet("/referrals/{id:guid}", GetReferral).RequireAuthorization();
        api.MapGet("/referrals/{id:guid}/history", ReferralHistory).RequireAuthorization();
        api.MapPost("/referrals/{id:guid}/outcomes", CreateOutcome).RequireAuthorization();
    }

    private static void MapOutcomeRoutes(this RouteGroupBuilder api)
    {
        api.MapGet("/outcomes", ListOutcomes).RequireAuthorization();
        api.MapGet("/outcomes/{id:guid}", GetOutcome).RequireAuthorization();
        api.MapPut("/outcomes/{id:guid}", UpdateOutcome).RequireAuthorization();
        api.MapPost("/outcomes/{id:guid}/submit", SubmitOutcome).RequireAuthorization();
        api.MapPost("/outcomes/{id:guid}/reviews", ReviewOutcome).RequireAuthorization();
    }
}
