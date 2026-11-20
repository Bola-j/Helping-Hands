using System.Net;
using HelpingHands.Api.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task LegacyStandardMapping_RejectsDraftStandardVersion()
    {
        using var beneficiary = await fixture.LoginAsync("Beneficiary");
        using var coordinator = await fixture.LoginAsync("Coordinator");
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var (_, _, standard, _) = await PublishedCatalogAsync(db);
        var draft = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow.AddDays(-1) };
        db.StandardVersions.Add(draft);
        await db.SaveChangesAsync();
        var request = await WorkflowDbFixture.AcceptedRequestAsync(beneficiary, coordinator);
        using var response = await WorkflowDbFixture.PostAsync(coordinator, $"/api/v1/requests/{request.GetProperty("id").GetGuid()}/legacy-standard", new
        { StandardVersionId = draft.Id, Version = request.GetProperty("version").GetGuid() });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
