using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    internal static async Task<IReadOnlyList<StaleCharityTarget>> FindStaleCharitiesAsync(
        HelpingHandsDbContext database, DateOnly cairoDate, CancellationToken cancellationToken)
    {
        var services = await LoadEnabledServicesAsync(database, cancellationToken);
        if (services.Count == 0) return [];
        var fresh = await LoadFreshServiceIdsAsync(database, services, cairoDate, cancellationToken);
        return GroupStaleServices(services, fresh);
    }

    private static Task<List<EnabledService>> LoadEnabledServicesAsync(
        HelpingHandsDbContext database, CancellationToken cancellationToken) =>
        GetEnabledServices(database).ToListAsync(cancellationToken);

    private static IQueryable<EnabledService> GetEnabledServices(HelpingHandsDbContext database) =>
        from cs in database.CharityServices.AsNoTracking()
        join recipient in database.Recipients.AsNoTracking() on cs.CharityId equals recipient.Id
        join charity in database.Charities.AsNoTracking() on cs.CharityId equals charity.RecipientId
        join service in database.ServiceVersions.AsNoTracking() on cs.ServiceVersionId equals service.Id
        join standard in database.StandardVersions.AsNoTracking() on cs.StandardVersionId equals standard.Id
        join unit in database.UnitStandards.AsNoTracking() on standard.UnitStandardId equals unit.Id
        where cs.Enabled && recipient.RecipientType == "Charity"
            && recipient.ApprovalStatus == "Approved" && recipient.Active
        select new EnabledService(charity.RecipientId, recipient.DisplayName, recipient.ContactDetails,
            cs.CharityServiceId, service.Name, service.VersionNumber, standard.VersionNumber, unit.Name);
}
