using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static Task RejectRedirect(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }

    private static async Task<IResult> Logout(SignInManager<AppUser> manager)
    {
        await manager.SignOutAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUser(ClaimsPrincipal principal, UserManager<AppUser> manager)
    {
        var user = await manager.GetUserAsync(principal);
        return user is null || user.AccountStatus != "Active"
            ? Results.Unauthorized()
            : Results.Ok(new { user.Id, user.DisplayName, user.Email, roles = await manager.GetRolesAsync(user) });
    }
}
