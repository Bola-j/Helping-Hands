using Microsoft.AspNetCore.WebUtilities;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static bool TryRequestCursor(string? cursor, out Guid? after)
    {
        after = null;
        if (cursor is null) return true;
        if (cursor.Length != 22) return false;
        try
        {
            var bytes = WebEncoders.Base64UrlDecode(cursor);
            if (bytes.Length != 16 || WebEncoders.Base64UrlEncode(bytes) != cursor) return false;
            after = new Guid(bytes);
            return true;
        }
        catch (FormatException) { return false; }
    }
}
