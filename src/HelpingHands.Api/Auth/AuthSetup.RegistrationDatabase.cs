using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static async Task<IdentityResult?> CreateRegisteredUserAsync(UserManager<AppUser> users, AppUser user, string password)
    {
        try { return await users.CreateAsync(user, password); }
        catch (DbUpdateException exception) when (IsDuplicateEmail(exception)) { return null; }
    }

    private static bool IsDuplicateEmail(DbUpdateException exception) =>
        exception.GetBaseException() is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_UserAccount_NormalizedEmail"
        };
}
