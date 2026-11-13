using System.Data;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Needs;

// The caller owns the transaction and must commit it with the new request and decision history.
public sealed partial class NeedVersionService(HelpingHandsDbContext db)
{
    public async Task<NeedCreationResult> AddNeedsAsync(Guid requestId, IReadOnlyList<NeedSelection> needs, CancellationToken ct = default)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("A transaction is required.");
        if (db.Database.CurrentTransaction.GetDbTransaction().IsolationLevel != IsolationLevel.ReadCommitted) throw new InvalidOperationException("Read committed isolation is required.");
        if (needs.Count == 0 || needs.Any(x => x.Quantity <= 0 || decimal.Round(x.Quantity, 3) != x.Quantity || x.Quantity >= 1_000_000_000_000_000m) || needs.Select(x => x.StandardVersionId).Distinct().Count() != needs.Count)
            return NeedCreationResult.InvalidNeeds;
        var request = await db.Requests.FromSqlInterpolated($"SELECT * FROM \"AssistanceRequest\" WHERE \"Id\" = {requestId} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(ct);
        if (request is null || !request.IsMultiNeed || request.Status != "Submitted") return NeedCreationResult.InvalidRequest;
        if (await db.RequestNeeds.AnyAsync(x => x.RequestId == requestId, ct)) return NeedCreationResult.InvalidRequest;
        var selections = await GetSelectionsAsync(needs, ct);
        if (selections.Count != needs.Count) return NeedCreationResult.StaleSelection;
        await LockCatalogAsync(selections, ct);
        if (!await AreCurrentAsync(selections, DateTimeOffset.UtcNow, ct)) return NeedCreationResult.StaleSelection;
        db.RequestNeeds.AddRange(needs.Select(x => new RequestNeed { RequestId = requestId, StandardVersionId = x.StandardVersionId, Quantity = x.Quantity }));
        await db.SaveChangesAsync(ct);
        return NeedCreationResult.Created;
    }
}
