using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    [Fact]
    public async Task CharityServiceAndDailyCapacity_EnforceVersionMatchAndUniqueness()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var charity = await CreateApprovedCharityAsync(db);
        var (_, serviceVersion, _, standardVersion) = await PublishedCatalogAsync(db);
        var (_, otherServiceVersion, _, _) = await PublishedCatalogAsync(db);

        db.CharityServices.Add(new CharityService { CharityId = charity.RecipientId, ServiceVersionId = otherServiceVersion.Id, StandardVersionId = standardVersion.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();

        var service = new CharityService { CharityId = charity.RecipientId, ServiceVersionId = serviceVersion.Id, StandardVersionId = standardVersion.Id };
        db.CharityServices.Add(service);
        await db.SaveChangesAsync();

        var date = new DateOnly(2026, 11, 17);
        db.DailyCapacities.Add(new DailyCapacity { CharityServiceId = service.CharityServiceId, CairoDate = date, Available = 10, UpdatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        db.DailyCapacities.Add(new DailyCapacity { CharityServiceId = service.CharityServiceId, CairoDate = date, Available = 5, UpdatedAt = DateTimeOffset.UtcNow });
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(ex.InnerException).SqlState);
    }

    [Fact]
    public async Task DailyCapacityAndCapacityUpdate_EnforceNonNegativeAndAppendOnly()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var charity = await CreateApprovedCharityAsync(db);
        var (_, serviceVersion, _, standardVersion) = await PublishedCatalogAsync(db);
        var service = new CharityService { CharityId = charity.RecipientId, ServiceVersionId = serviceVersion.Id, StandardVersionId = standardVersion.Id };
        db.CharityServices.Add(service);
        var capacity = new DailyCapacity { CharityServiceId = service.CharityServiceId, CairoDate = new DateOnly(2026, 11, 18), Available = 25, UpdatedAt = DateTimeOffset.UtcNow };
        db.DailyCapacities.Add(capacity);
        var update = new CapacityUpdate { CapacityId = capacity.CapacityId, ActorId = fixture.UserIds["Coordinator"], Available = 25, UpdatedAt = DateTimeOffset.UtcNow };
        db.CapacityUpdates.Add(update);
        await db.SaveChangesAsync();

        await RejectAsync(db, $"UPDATE \"DailyCapacity\" SET \"Available\" = -1 WHERE \"CapacityId\" = '{capacity.CapacityId}'");
        await RejectAsync(db, $"UPDATE \"CapacityUpdate\" SET \"Available\" = 99 WHERE \"CapacityUpdateId\" = '{update.CapacityUpdateId}'");
        await RejectAsync(db, $"DELETE FROM \"CapacityUpdate\" WHERE \"CapacityUpdateId\" = '{update.CapacityUpdateId}'");
    }

   private static async Task<Charity> CreateApprovedCharityAsync(HelpingHandsDbContext db)
    {
        var recipient = new Recipient
        {
            Reference = await db.NextReferenceAsync("REC"),
            RecipientType = "Charity",
            DisplayName = "Fictional Capacity Charity",
            ContactDetails = "capacity@example.test",
            ApprovalStatus = "Approved",
            Active = true
        };
        var charity = new Charity { RecipientId = recipient.Id, RegistrationInformation = "REG-18" };
        db.Recipients.Add(recipient);
        db.Charities.Add(charity);
        await db.SaveChangesAsync();
        return charity;
    }
}