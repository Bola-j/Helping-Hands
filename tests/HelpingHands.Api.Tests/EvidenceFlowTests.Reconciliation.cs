using HelpingHands.Api.Data;
using HelpingHands.Api.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HelpingHands.Api.Tests;

public sealed partial class EvidenceFlowTests
{
    private async Task AssertOrphanReconciliationAsync(Guid outcomeId)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IEvidenceStorage>();
        var evidenceKey = await database.EvidenceAttachments.AsNoTracking()
            .Where(item => item.OutcomeId == outcomeId).Select(item => item.StorageKey).SingleAsync();
        var retainedKeys = await database.ApplicationAttachments.AsNoTracking()
            .Select(item => item.StorageKey).ToListAsync();
        retainedKeys.Add(evidenceKey);
        using var content = new MemoryStream("%PDF-1.4\norphan fixture\n%%EOF"u8.ToArray());
        var stagedKey = await storage.StageAsync(content, "application/pdf", CancellationToken.None);
        var orphanKey = await storage.PromoteAsync(stagedKey, CancellationToken.None);
        var reconciler = new EvidenceStorageReconciliationService(database, storage,
            Options.Create(new EvidenceStorageOptions { OrphanRetentionHours = 24 }),
            new FixedTimeProvider(DateTimeOffset.UtcNow.AddDays(2)));

        await reconciler.ReconcileAsync(CancellationToken.None);

        var keys = await PrivateKeysAsync();
        foreach (var retainedKey in retainedKeys)
            Assert.Contains(retainedKey, keys);
        Assert.DoesNotContain(orphanKey, keys);
    }
}
