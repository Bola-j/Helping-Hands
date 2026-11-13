using System.Net;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task MultiNeedSubmissionWaitsForPublicationAndRejectsTheStaleVersion()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var (db, profile) = await LinkedProfileAsync(scope);
        var (_, _, standard, first) = await PublishedCatalogAsync(db);
        var next = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 2, EffectiveAt = DateTimeOffset.UtcNow.AddMinutes(-1) };
        db.StandardVersions.Add(next);
        await db.SaveChangesAsync();
        await using var publisher = new NpgsqlConnection(db.Database.GetConnectionString());
        await publisher.OpenAsync();
        await using var tx = await publisher.BeginTransactionAsync();
        await new NpgsqlCommand($"UPDATE \"StandardVersion\" SET \"IsPublished\" = TRUE WHERE \"Id\" = '{next.Id}'", publisher, tx).ExecuteNonQueryAsync();
        using var client = await fixture.LoginAsync("Beneficiary");
        var before = await db.Requests.CountAsync(x => x.BeneficiaryId == profile.Id);
        var submit = PostNeedsAsync(client, profile.Id, new[] { new { StandardVersionId = first.Id, Quantity = 1m } });
        await Task.Delay(200);
        Assert.False(submit.IsCompleted);
        await tx.CommitAsync();
        using var response = await submit.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(before, await db.Requests.CountAsync(x => x.BeneficiaryId == profile.Id));
    }
}
