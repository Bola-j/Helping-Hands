using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task NeedMigrationPreservesLegacyRequestReferralOutcomeAndHistory()
    {
        var connection = Environment.GetEnvironmentVariable("P1_WORKFLOW_POSTGRES")!;
        var name = "helping_hands_workflow_needs_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(connection);
        await admin.OpenAsync();
        await new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin).ExecuteNonQueryAsync();
        var builder = new NpgsqlConnectionStringBuilder(connection) { Database = name, Pooling = false };
        try { await VerifyLegacyUpgradeAsync(builder.ConnectionString); }
        finally { await new NpgsqlCommand($"DROP DATABASE \"{name}\"", admin).ExecuteNonQueryAsync(); }
    }

    private static async Task VerifyLegacyUpgradeAsync(string connection)
    {
        await using var db = new HelpingHandsDbContext(new DbContextOptionsBuilder<HelpingHandsDbContext>().UseNpgsql(connection).Options);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261113125257_PreservePublishedCatalogVersions");
        var user = new AppUser { UserName = "fictional", NormalizedUserName = "FICTIONAL", DisplayName = "Fictional" };
        var beneficiary = new Beneficiary { CreatedByUserId = user.Id, Reference = "BEN-00000001", DisplayName = "Fictional" };
        var recipient = new Recipient { Reference = "REC-00000001", DisplayName = "Fictional", ContactDetails = "Fictional" };
        db.AddRange(user, beneficiary, recipient);
        await db.SaveChangesAsync();
        var requestId = Guid.NewGuid(); var requestVersion = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "AssistanceRequest" ("Id", "Reference", "BeneficiaryId", "SubmittedByUserId", "Category", "Description", "RequiredQuantity", "Unit", "Status", "AuthorityConfirmed", "SharingAgreed", "CreatedAt", "Version")
            VALUES ({requestId}, 'REQ-00000001', {beneficiary.Id}, {user.Id}, 'Goods', 'Fictional', 2, 'kg', 'Open', TRUE, TRUE, now(), {requestVersion})
            """);
        var referral = new Referral { RequestId = requestId, RecipientId = recipient.Id, CreatedByUserId = user.Id, Reference = "REF-00000001", AssignedQuantity = 2, ScopeDescription = "Fictional" };
        var outcome = new Outcome { ReferralId = referral.Id, ReportedByUserId = user.Id, Reference = "OUT-00000001", ReportedQuantity = 1, Description = "Fictional" };
        var history = new DecisionHistory { RequestId = requestId, ActorUserId = user.Id, Action = "Accepted" };
        db.AddRange(referral, outcome, history);
        await db.SaveChangesAsync();
        await migrator.MigrateAsync("20261113133748_RequestNeeds");
        var multiRequest = Guid.NewGuid(); var category = Guid.NewGuid(); var service = Guid.NewGuid(); var standard = Guid.NewGuid(); var version = Guid.NewGuid(); var need = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "AssistanceRequest" ("Id", "Reference", "BeneficiaryId", "SubmittedByUserId", "Description", "Status", "AuthorityConfirmed", "SharingAgreed", "CreatedAt", "Version", "IsMultiNeed")
            VALUES ({multiRequest}, 'REQ-00000002', {beneficiary.Id}, {user.Id}, 'Existing multi need', 'Submitted', TRUE, TRUE, now(), {Guid.NewGuid()}, TRUE);
            INSERT INTO "ServiceCategory" ("Id", "Code") VALUES ({category}, 'FOOD');
            INSERT INTO "ServiceVersion" ("Id", "CategoryId", "VersionNumber", "Name", "IsPublished") VALUES ({service}, {category}, 1, 'Food', FALSE);
            UPDATE "ServiceVersion" SET "IsPublished" = TRUE WHERE "Id" = {service};
            INSERT INTO "UnitStandard" ("Id", "ServiceVersionId", "Name") VALUES ({standard}, {service}, 'Parcel');
            INSERT INTO "StandardVersion" ("Id", "UnitStandardId", "VersionNumber", "EffectiveAt", "IsPublished") VALUES ({version}, {standard}, 1, now(), FALSE);
            UPDATE "StandardVersion" SET "IsPublished" = TRUE WHERE "Id" = {version};
            INSERT INTO "RequestNeed" ("Id", "RequestId", "StandardVersionId", "Quantity", "Status") VALUES ({need}, {multiRequest}, {version}, 1, 'PreviouslyAllowed');
            """);
        await migrator.MigrateAsync();
        db.ChangeTracker.Clear();
        var saved = await db.Requests.SingleAsync(x => x.Id == requestId);
        Assert.False(saved.IsMultiNeed);
        Assert.Equal(2, saved.RequiredQuantity);
        var retainedReferral = await db.Referrals.SingleAsync(x => x.Id == referral.Id);
        Assert.Equal(requestId, retainedReferral.RequestId);
        Assert.Null(retainedReferral.NeedId);
        Assert.Equal(referral.Id, (await db.Outcomes.SingleAsync(x => x.Id == outcome.Id)).ReferralId);
        Assert.Equal(requestId, (await db.DecisionHistory.SingleAsync(x => x.Id == history.Id)).RequestId);
        var retainedNeed = await db.RequestNeeds.SingleAsync(x => x.Id == need);
        Assert.Equal("PreviouslyAllowed", retainedNeed.Status);
        Assert.Equal(multiRequest, retainedNeed.RequestId);
        Assert.Equal(version, retainedNeed.StandardVersionId);
    }
}
