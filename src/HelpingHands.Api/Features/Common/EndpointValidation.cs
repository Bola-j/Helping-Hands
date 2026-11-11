using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static IResult Conflict(string message) => Results.Problem(statusCode: 409, title: "Conflict", detail: message);

    private static bool Precision(decimal value) => decimal.Round(value, 3) == value && value < 1_000_000_000_000_000m;

    private static async Task<bool> Member(HelpingHandsDbContext database, Guid recipientId, Guid userId, CancellationToken cancellationToken) =>
            await database.RecipientMembers.AnyAsync(item => item.RecipientId == recipientId && item.UserId == userId && item.IsActive, cancellationToken) && await (from userRole in database.UserRoles join role in database.Roles on userRole.RoleId equals role.Id where userRole.UserId == userId && role.NormalizedName == "RECIPIENTREPRESENTATIVE" select userRole.Id).AnyAsync(cancellationToken);
}
