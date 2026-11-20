using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Reminders;

public sealed partial class CairoCapacityReminderRunner
{
    private static async Task<HashSet<Guid>> LoadFreshServiceIdsAsync(
        HelpingHandsDbContext database, IReadOnlyCollection<EnabledService> services,
        DateOnly date, CancellationToken cancellationToken)
    {
        var ids = services.Select(service => service.CharityServiceId).ToList();
        var fresh = await database.DailyCapacities.AsNoTracking()
            .Where(capacity => ids.Contains(capacity.CharityServiceId) && capacity.CairoDate == date)
            .Select(capacity => capacity.CharityServiceId)
            .ToListAsync(cancellationToken);
        return fresh.ToHashSet();
    }

    private static IReadOnlyList<StaleCharityTarget> GroupStaleServices(
        IEnumerable<EnabledService> services, HashSet<Guid> freshIds) =>
        services.Where(service => !freshIds.Contains(service.CharityServiceId))
            .GroupBy(service => new CharityGroupKey(service.CharityId, service.DisplayName, service.ContactDetails))
            .OrderBy(group => group.Key.CharityId)
            .Select(group => ToStaleTarget(group.Key, group))
            .ToList();

    private static StaleCharityTarget ToStaleTarget(
        CharityGroupKey key, IEnumerable<EnabledService> services) =>
        new(key.CharityId, key.DisplayName, key.ContactDetails,
            services.Select(service => $"{service.ServiceName} v{service.VersionNumber} ({service.UnitName} std v{service.StandardVersionNumber})").ToList());
}
