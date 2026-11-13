using System.Net;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task MultiNeedSubmissionAcceptsFiftyNeedsAndRejectsFiftyOne()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, profile) = await LinkedProfileAsync(scope);
        var (_, service, _, _) = await PublishedCatalogAsync(db);
        var versions = Enumerable.Range(1, 50).Select(i =>
        {
            var standard = new UnitStandard { ServiceVersionId = service.Id, Name = $"Fictional standard {i}" };
            var version = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 1, EffectiveAt = DateTimeOffset.UtcNow.AddDays(-1) };
            db.UnitStandards.Add(standard);
            db.StandardComponents.Add(new StandardComponent { StandardVersionId = version.Id, Name = "Rice", Quantity = 1, Unit = "kg" });
            return version;
        }).ToArray();
        db.StandardVersions.AddRange(versions);
        await db.SaveChangesAsync();
        foreach (var version in versions) version.IsPublished = true;
        await db.SaveChangesAsync();
        using var client = await fixture.LoginAsync("Beneficiary");
        var needs = versions.Select(x => new { StandardVersionId = x.Id, Quantity = 1m }).ToArray();
        using var accepted = await PostNeedsAsync(client, profile.Id, needs);
        var result = await WorkflowDbFixture.JsonAsync(accepted, HttpStatusCode.Created);
        Assert.Equal(50, result.GetProperty("needs").GetArrayLength());
        var before = await db.Requests.CountAsync(x => x.BeneficiaryId == profile.Id);
        using var rejected = await PostNeedsAsync(client, profile.Id, needs.Append(needs[0]).ToArray());
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Equal(before, await db.Requests.CountAsync(x => x.BeneficiaryId == profile.Id));
    }
}
