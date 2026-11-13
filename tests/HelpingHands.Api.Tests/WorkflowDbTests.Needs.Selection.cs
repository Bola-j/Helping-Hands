using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task NeedSelectionRejectsStaleAndPreservesRecordedHistory()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (category, service, standard, first) = await PublishedCatalogAsync(db);
        var selection = scope.ServiceProvider.GetRequiredService<NeedVersionService>();
        var draft = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 9, EffectiveAt = DateTimeOffset.UtcNow.AddDays(-1) };
        db.StandardVersions.Add(draft);
        await db.SaveChangesAsync();
        var future = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow.AddDays(3) };
        await PublishAsync(db, future);
        db.ServiceVersions.Add(new ServiceVersion { CategoryId = category.Id, VersionNumber = 9, Name = "Draft service" });
        await db.SaveChangesAsync();
        Assert.Null(await selection.FindCurrentAsync(category.Id, standard.Id, DateTimeOffset.UtcNow.AddDays(-4)));
        Assert.Equal(first.Id, await selection.FindCurrentAsync(category.Id, standard.Id, DateTimeOffset.UtcNow));
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            Assert.Equal(NeedCreationResult.Created, await selection.AddNeedsAsync(request.Id, [new(first.Id, 2)]));
            await tx.CommitAsync();
        }
        var second = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 3, EffectiveAt = DateTimeOffset.UtcNow.AddDays(-1) };
        await PublishAsync(db, second);
        var tied = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 4, EffectiveAt = second.EffectiveAt };
        await PublishAsync(db, tied);
        Assert.Equal(tied.Id, await selection.FindCurrentAsync(category.Id, standard.Id, DateTimeOffset.UtcNow));
        var saved = Assert.Single(await selection.GetRecordedNeedsAsync(request.Id));
        Assert.Equal(first.Id, saved.StandardVersionId);
        Assert.Equal(service.Id, saved.ServiceVersionId);
        Assert.Equal("Rice", Assert.Single(saved.Components).Name);
        var newerService = new ServiceVersion { CategoryId = category.Id, VersionNumber = 2, Name = "Food revised" };
        db.ServiceVersions.Add(newerService);
        await db.SaveChangesAsync();
        newerService.IsPublished = true;
        await db.SaveChangesAsync();
        Assert.Null(await selection.FindCurrentAsync(category.Id, standard.Id, DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task NeedCreationRejectsInvalidOrStaleSelectionsAndDuplicates()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, request) = await NeedRequestAsync(scope);
        var (_, _, standard, first) = await PublishedCatalogAsync(db);
        var next = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow.AddHours(-1) };
        await PublishAsync(db, next);
        var selector = scope.ServiceProvider.GetRequiredService<NeedVersionService>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => selector.AddNeedsAsync(request.Id, [new(next.Id, 1)]));
        await using var tx = await db.Database.BeginTransactionAsync();
        Assert.Equal(NeedCreationResult.StaleSelection, await selector.AddNeedsAsync(request.Id, [new(first.Id, 1)]));
        Assert.Equal(NeedCreationResult.StaleSelection, await selector.AddNeedsAsync(request.Id, [new(Guid.NewGuid(), 1)]));
        Assert.Equal(NeedCreationResult.InvalidNeeds, await selector.AddNeedsAsync(request.Id, [new(next.Id, 0)]));
        Assert.Equal(NeedCreationResult.InvalidNeeds, await selector.AddNeedsAsync(request.Id, [new(next.Id, 1), new(next.Id, 2)]));
        Assert.Equal(0, await db.RequestNeeds.CountAsync(x => x.RequestId == request.Id));
    }
}
