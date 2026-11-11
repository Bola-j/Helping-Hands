namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<bool> HasExpectedSignature(IFormFile file, CancellationToken ct)
    {
        var header = new byte[8];
        await using var stream = file.OpenReadStream();
        var count = await stream.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        return file.ContentType switch
        {
            "application/pdf" => count >= 5 && header.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
            "image/jpeg" => count >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => count == 8 && header.AsSpan().SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            _ => false
        };
    }
}
