using HelpingHands.Api.Data;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static IQueryable<Guid> CurrentRepresentativeUserIds(HelpingHandsDbContext database) =>
        from userRole in database.UserRoles
        join role in database.Roles on userRole.RoleId equals role.Id
        where role.NormalizedName == "RECIPIENTREPRESENTATIVE"
        select userRole.UserId;
}
