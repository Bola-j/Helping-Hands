using System.Security.Cryptography;
using System.Text.Json;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static string CatalogTag<T>(T item) => "\"" + Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(item))) + "\"";
    private static IResult? CheckTag<T>(HttpRequest request, T item)
    {
        if (!request.Headers.TryGetValue("If-Match", out var supplied)) return Results.Problem(statusCode: 428, title: "If-Match is required.");
        return supplied.ToString() == CatalogTag(item) ? null : Results.Problem(statusCode: 409, title: "The catalog item has changed.");
    }
    private static IResult CatalogResult<T>(HttpResponse response, T item, string? location = null)
    {
        response.Headers.ETag = CatalogTag(item);
        return location is null ? Results.Ok(item) : Results.Created(location, item);
    }
}
