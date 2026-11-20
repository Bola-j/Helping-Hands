using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult?> ValidateCharityDailyCapacityFreshnessAsync(HelpingHandsDbContext database,
        AssistanceRequest request, ReferralInput input, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        if (request.LegacyStandardVersionId is not Guid standardId)
            return Conflict("Map the request to a service standard before creating a referral.");
        if (input.ServiceVersionId.HasValue != input.StandardVersionId.HasValue)
            return Bad("Both serviceVersionId and standardVersionId must be provided together.");
        var serviceId = await MappedServiceVersionAsync(database, standardId, cancellationToken);
        if (serviceId is null || (input.StandardVersionId.HasValue &&
            (input.StandardVersionId != standardId || input.ServiceVersionId != serviceId)))
            return Conflict("The selected service version does not match the request mapping.");
        var charity = await database.Charities.FromSqlInterpolated(
            $"SELECT * FROM \"Charity\" WHERE \"RecipientId\" = {input.RecipientId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (charity is null) return Conflict("Recipient does not support the mapped service.");
        return await CheckMappedCapacityAsync(database, input.RecipientId, standardId, serviceId.Value, timeProvider, cancellationToken);
    }

    private static async Task<Guid?> MappedServiceVersionAsync(HelpingHandsDbContext database, Guid standardId, CancellationToken ct) =>
        await (from version in database.StandardVersions.AsNoTracking()
               join standard in database.UnitStandards.AsNoTracking() on version.UnitStandardId equals standard.Id
               where version.Id == standardId
               select (Guid?)standard.ServiceVersionId).SingleOrDefaultAsync(ct);

    private static async Task<IResult?> CheckMappedCapacityAsync(HelpingHandsDbContext database, Guid charityId,
        Guid standardId, Guid serviceId, TimeProvider timeProvider, CancellationToken ct)
    {
        var selection = await database.CharityServices.AsNoTracking().Where(item => item.CharityId == charityId && item.Enabled &&
            item.ServiceVersionId == serviceId && item.StandardVersionId == standardId)
            .Select(item => (Guid?)item.CharityServiceId).SingleOrDefaultAsync(ct);
        if (selection is null) return Conflict("Recipient does not support the mapped service version.");
        var today = GetCurrentCairoDate(timeProvider.GetUtcNow());
        var fresh = await database.DailyCapacities.AsNoTracking().AnyAsync(item => item.CharityServiceId == selection && item.CairoDate == today, ct);
        return fresh ? null : Conflict("Daily capacity for this service version is stale and frozen until updated today (Cairo time).");
    }
}
