using System.ComponentModel.DataAnnotations;
using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;

namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static bool IsValidRegistration(RegisterRequest request) =>
        !string.IsNullOrWhiteSpace(request.DisplayName) && request.DisplayName.Trim().Length <= 200 &&
        !string.IsNullOrWhiteSpace(request.Email) && request.Email.Length <= 320 &&
        new EmailAddressAttribute().IsValid(request.Email) && !string.IsNullOrEmpty(request.Password);

    private static AppUser NewUser(string email, string displayName) => new()
    {
        Id = Guid.NewGuid(),
        UserName = Guid.NewGuid().ToString(),
        Email = email,
        DisplayName = displayName,
        AccountStatus = "Active",
        LockoutEnabled = true
    };

    private static IResult RegistrationErrors(IdentityResult result) =>
        Results.ValidationProblem(result.Errors.GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
}
