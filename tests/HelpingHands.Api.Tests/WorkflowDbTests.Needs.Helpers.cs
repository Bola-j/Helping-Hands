using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<(HelpingHandsDbContext Db, AssistanceRequest Request)> NeedRequestAsync(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var user = fixture.UserIds["Beneficiary"];
        var beneficiary = new Beneficiary { CreatedByUserId = user, DisplayName = "Fictional", Reference = await db.NextReferenceAsync("BEN") };
        var request = new AssistanceRequest { BeneficiaryId = beneficiary.Id, SubmittedByUserId = user, Reference = await db.NextReferenceAsync("REQ"), Description = "Fictional", AuthorityConfirmed = true, SharingAgreed = true, IsMultiNeed = true };
        db.AddRange(beneficiary, request);
        await db.SaveChangesAsync();
        return (db, request);
    }

    private static async Task<(ServiceCategory Category, ServiceVersion Service, UnitStandard Standard, StandardVersion Version)> PublishedCatalogAsync(HelpingHandsDbContext db)
    {
        var category = new ServiceCategory { Code = Guid.NewGuid().ToString("N") };
        var service = new ServiceVersion { CategoryId = category.Id, VersionNumber = 1, Name = "Food" };
        var standard = new UnitStandard { ServiceVersionId = service.Id, Name = "Parcel" };
        var version = new StandardVersion { UnitStandardId = standard.Id, VersionNumber = 1, EffectiveAt = DateTimeOffset.UtcNow.AddDays(-3) };
        db.AddRange(category, service, standard, version, new StandardComponent { StandardVersionId = version.Id, Name = "Rice", Quantity = 1, Unit = "kg" });
        await db.SaveChangesAsync();
        service.IsPublished = true;
        await db.SaveChangesAsync();
        version.IsPublished = true;
        await db.SaveChangesAsync();
        return (category, service, standard, version);
    }

    private static async Task PublishAsync(HelpingHandsDbContext db, StandardVersion version)
    {
        db.StandardVersions.Add(version);
        await db.SaveChangesAsync();
        version.IsPublished = true;
        await db.SaveChangesAsync();
    }
}
