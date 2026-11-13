using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;

namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static async Task<IResult> RegisterBeneficiary(BeneficiarySignupRequest request, UserManager<AppUser> users, RoleManager<AppRole> roles, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!IsValidBeneficiarySignup(request)) return InvalidSignup("Provide a valid name, email, password and contact details.");
        if (!await roles.RoleExistsAsync("Beneficiary")) return RolesUnavailable();
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var user = NewUser(request.Email!.Trim(), request.DisplayName!.Trim());
        var result = await CreateRegisteredUserAsync(users, user, request.Password!);
        if (result is null) return RegistrationErrors(DuplicateEmail());
        if (!result.Succeeded) return RegistrationErrors(result);
        result = await users.AddToRoleAsync(user, "Beneficiary");
        if (!result.Succeeded) return RoleAssignmentFailed();
        var beneficiary = await NewLinkedBeneficiaryAsync(request, user.Id, database, cancellationToken);
        database.Beneficiaries.Add(beneficiary);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Created($"/api/v1/beneficiaries/{beneficiary.Id}", new { accountId = user.Id, beneficiaryId = beneficiary.Id, beneficiary.Reference, user.DisplayName, user.Email });
    }

    private static bool IsValidBeneficiarySignup(BeneficiarySignupRequest request) =>
        request.DisplayName is not null && request.Email is not null && request.Password is not null &&
        IsValidRegistration(request.DisplayName, request.Email, request.Password) && (request.ContactDetails?.Length ?? 0) <= 500;

    private static async Task<Beneficiary> NewLinkedBeneficiaryAsync(BeneficiarySignupRequest request, Guid userId, HelpingHandsDbContext database, CancellationToken cancellationToken) =>
        new() { LinkedUserId = userId, CreatedByUserId = userId, DisplayName = request.DisplayName!.Trim(), ContactDetails = request.ContactDetails?.Trim(), IsFamily = request.IsFamily, Reference = await database.NextReferenceAsync("BEN", cancellationToken) };
}
