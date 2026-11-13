using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HelpingHands.Api.Features.Catalog;

public static partial class CatalogEndpoints
{
    private static bool InvalidText(string? text, int limit) => string.IsNullOrWhiteSpace(text) || text.Trim().Length > limit;
    private static bool InvalidComponent(ComponentInput input) => InvalidText(input.Name, 200) || InvalidText(input.Unit, 50) || input.Quantity <= 0 || input.Quantity >= 1_000_000_000_000_000m || decimal.Round(input.Quantity, 3) != input.Quantity;
    private static async Task<IResult?> SaveCatalogAsync(HelpingHandsDbContext db, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); return null; }
        catch (DbUpdateException error) when (error.InnerException is PostgresException pg && (pg.SqlState is PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation or PostgresErrorCodes.CheckViolation or PostgresErrorCodes.RaiseException))
        { return Results.Problem(statusCode: 409, title: "The catalog changed or the proposed values conflict."); }
    }
}
