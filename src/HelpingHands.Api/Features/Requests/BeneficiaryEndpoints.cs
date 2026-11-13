using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> CreateBeneficiary(BeneficiaryInput input, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!Beneficiary(user)) return Results.Forbid();
        if (Blank(input.DisplayName, 200) || input.ContactDetails?.Length > 500) return Bad("Valid beneficiary details are required.");
        var beneficiary = new Beneficiary { CreatedByUserId = Actor(user), DisplayName = input.DisplayName.Trim(), ContactDetails = input.ContactDetails, IsFamily = input.IsFamily, Reference = await database.NextReferenceAsync("BEN", cancellationToken) };
        // A beneficiary may be independent of any user. Access derives from submitted requests.
        database.Beneficiaries.Add(beneficiary);
        await database.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/v1/beneficiaries/{beneficiary.Id}", new { beneficiary.Id, beneficiary.Reference, beneficiary.DisplayName, beneficiary.IsFamily });
    }

    private static async Task<IResult> ListBeneficiaries(string? reference, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var query = VisibleBeneficiaries(database, user);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(item => item.Reference == reference);
        return Results.Ok(await query.OrderBy(item => item.Id).Take(100).Select(beneficiary => new { beneficiary.Id, beneficiary.Reference, beneficiary.DisplayName, beneficiary.IsFamily }).ToListAsync(cancellationToken));
    }

    private static async Task<IResult> GetBeneficiary(Guid id, ClaimsPrincipal user, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var beneficiary = await VisibleBeneficiaries(database, user).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (beneficiary is null) return Results.NotFound();
        return Results.Ok(new { beneficiary.Id, beneficiary.Reference, beneficiary.DisplayName, beneficiary.IsFamily });
    }
}
