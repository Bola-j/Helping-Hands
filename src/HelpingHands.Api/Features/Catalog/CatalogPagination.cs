using System.Text;

namespace HelpingHands.Api.Features.Catalog;

public sealed record CatalogPage<T>(IReadOnlyList<T> Items, string? NextCursor);

public static partial class CatalogEndpoints
{
    private static bool TryCatalogCursor(string? cursor, string scope, int? limit, out Guid? after)
    {
        after = null;
        if (limit is < 1 or > 100) return false;
        if (cursor is null) return true;
        if (cursor.Length is < 1 or > 300) return false;
        try
        {
            var value = cursor.Replace('-', '+').Replace('_', '/');
            value = value.PadRight((value.Length + 3) / 4 * 4, '=');
            var parts = Encoding.UTF8.GetString(Convert.FromBase64String(value)).Split('|');
            if (parts.Length != 2 || parts[0] != scope || !Guid.TryParseExact(parts[1], "N", out var id)) return false;
            after = id;
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static CatalogPage<T> CatalogPageResult<T>(List<T> rows, int size, string scope, Func<T, Guid> key)
    {
        var hasMore = rows.Count > size;
        var items = rows.Take(size).ToArray();
        var raw = hasMore ? Encoding.UTF8.GetBytes($"{scope}|{key(items[^1]):N}") : null;
        var cursor = raw is null ? null : Convert.ToBase64String(raw).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return new CatalogPage<T>(items, cursor);
    }
}
