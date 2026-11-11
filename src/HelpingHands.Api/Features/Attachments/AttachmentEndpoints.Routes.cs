namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static void MapAttachmentEndpoints(this RouteGroupBuilder api)
    {
        api.MapPost("/recipients/{id:guid}/attachments", UploadApplication).RequireAuthorization();
        api.MapPost("/outcomes/{id:guid}/attachments", UploadEvidence).RequireAuthorization();
        api.MapGet("/attachments/{id:guid}/content", DownloadAttachment).RequireAuthorization();
    }
}
