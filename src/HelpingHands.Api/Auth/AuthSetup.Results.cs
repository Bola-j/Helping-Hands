using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HelpingHands.Api.Auth;

public static partial class AuthSetup
{
    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException("Demo seed failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
