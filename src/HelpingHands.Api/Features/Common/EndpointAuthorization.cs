using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static bool Staff(ClaimsPrincipal user) => Coordinator(user);

    private static bool Blank(string? value, int max) => string.IsNullOrWhiteSpace(value) || value.Length > max;

    private static IResult Bad(string message) => Results.Problem(statusCode: 400, title: "Invalid request", detail: message);
}
