namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedTypes = ["application/pdf", "image/jpeg", "image/png"];

    private static async Task<IFormFile?> ReadValidAttachmentAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType || request.ContentLength is null or <= 0 or > MaxFileSize + 65536)
            return null;

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.Count == 1 ? form.Files[0] : null;
        if (file is not { Length: > 0 and <= MaxFileSize } ||
            !AllowedTypes.Contains(file.ContentType) || Blank(file.FileName, 255))
            return null;

        return await HasExpectedSignature(file, cancellationToken) ? file : null;
    }
}
