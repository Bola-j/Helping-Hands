using Microsoft.AspNetCore.Identity;

namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static IdentityResult DuplicateEmail() =>
        IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email is already registered." });
}
