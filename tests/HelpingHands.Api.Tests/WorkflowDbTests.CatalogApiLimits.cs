using System.Net;
using HelpingHands.Api.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task DraftStandardRejectsAdditionalComponentsAtLimit()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (_, _, standard, _) = await PublishedCatalogAsync(db);
        var draft = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow };
        db.StandardVersions.Add(draft);
        db.StandardComponents.AddRange(Enumerable.Range(0, 100).Select(i => new StandardComponent { StandardVersionId = draft.Id, Name = $"Item {i}", Quantity = 1, Unit = "kg" }));
        await db.SaveChangesAsync();
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var response = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/catalog/standard-versions/{draft.Id}/components", new { Name = "Overflow", Quantity = 1, Unit = "kg" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
