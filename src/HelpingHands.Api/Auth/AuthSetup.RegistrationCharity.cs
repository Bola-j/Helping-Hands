using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;

namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static async Task<IResult> RegisterCharity(CharitySignupRequest request, UserManager<AppUser> users, RoleManager<AppRole> roles, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        if (!IsValidCharitySignup(request)) return InvalidSignup("Provide a valid representative, email, password, organization and contact details.");
        if (!await roles.RoleExistsAsync("RecipientRepresentative")) return RolesUnavailable();
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var user = NewUser(request.Email!.Trim(), request.RepresentativeDisplayName!.Trim());
        var result = await CreateRegisteredUserAsync(users, user, request.Password!);
        if (result is null) return RegistrationErrors(DuplicateEmail());
        if (!result.Succeeded) return RegistrationErrors(result);
        result = await users.AddToRoleAsync(user, "RecipientRepresentative");
        if (!result.Succeeded) return RoleAssignmentFailed();
        var recipient = await NewPendingCharityAsync(request, user.Id, database, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Created($"/api/v1/recipients/{recipient.Id}", new { accountId = user.Id, recipientId = recipient.Id, recipient.Reference, recipient.DisplayName, recipient.ApprovalStatus, recipient.Active, user.Email });
    }

    private static bool IsValidCharitySignup(CharitySignupRequest request) =>
        request.RepresentativeDisplayName is not null && request.Email is not null && request.Password is not null &&
        request.OrganizationName is not null && request.ContactDetails is not null &&
        IsValidRegistration(request.RepresentativeDisplayName, request.Email, request.Password) &&
        !string.IsNullOrWhiteSpace(request.OrganizationName) && request.OrganizationName.Trim().Length <= 200 &&
        !string.IsNullOrWhiteSpace(request.ContactDetails) && request.ContactDetails.Trim().Length <= 500 && (request.RegistrationInformation?.Length ?? 0) <= 1000;

    private static async Task<Recipient> NewPendingCharityAsync(CharitySignupRequest request, Guid userId, HelpingHandsDbContext database, CancellationToken cancellationToken)
    {
        var recipient = new Recipient { Reference = await database.NextReferenceAsync("REC", cancellationToken), DisplayName = request.OrganizationName!.Trim(), ContactDetails = request.ContactDetails!.Trim() };
        database.Recipients.Add(recipient);
        database.Charities.Add(new Charity { RecipientId = recipient.Id, RegistrationInformation = request.RegistrationInformation?.Trim() });
        database.RecipientMembers.Add(new RecipientMember { RecipientId = recipient.Id, UserId = userId });
        return recipient;
    }
}
