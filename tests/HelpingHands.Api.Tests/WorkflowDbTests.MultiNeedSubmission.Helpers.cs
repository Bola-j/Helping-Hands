using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private async Task<(HelpingHandsDbContext Db, Beneficiary Profile)> LinkedProfileAsync(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<HelpingHandsDbContext>();
        var userId = fixture.UserIds["Beneficiary"];
        var profile = await db.Beneficiaries.SingleOrDefaultAsync(x => x.LinkedUserId == userId);
        if (profile is not null) return (db, profile);
        profile = new Beneficiary { LinkedUserId = userId, CreatedByUserId = userId, Reference = await db.NextReferenceAsync("BEN"), DisplayName = "Fictional" };
        db.Beneficiaries.Add(profile);
        await db.SaveChangesAsync();
        return (db, profile);
    }
}
