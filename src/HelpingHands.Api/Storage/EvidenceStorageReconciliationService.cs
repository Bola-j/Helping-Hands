using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpingHands.Api.Storage;

public sealed class EvidenceStorageReconciliationService(
    HelpingHandsDbContext database,
    IEvidenceStorage storage,
    IOptions<EvidenceStorageOptions> options,
    TimeProvider timeProvider)
{
    public async Task<int> ReconcileAsync(CancellationToken cancellationToken)
    {
        var evidenceKeys = await database.EvidenceAttachments.AsNoTracking()
            .Select(attachment => attachment.StorageKey).ToListAsync(cancellationToken);
        var applicationKeys = await database.ApplicationAttachments.AsNoTracking()
            .Select(attachment => attachment.StorageKey).ToListAsync(cancellationToken);
        var retainedKeys = evidenceKeys.Concat(applicationKeys).ToHashSet(StringComparer.Ordinal);
        var cutoff = timeProvider.GetUtcNow().AddHours(-options.Value.OrphanRetentionHours);
        var stagedCount = await storage.CleanupStagedBeforeAsync(cutoff, cancellationToken);
        var orphanCount = await storage.CleanupOrphanedFinalBeforeAsync(retainedKeys, cutoff, cancellationToken);
        return stagedCount + orphanCount;
    }
}
