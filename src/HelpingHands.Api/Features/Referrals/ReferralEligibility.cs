using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static bool InvalidReferralInput(ReferralInput input) =>
        input.AssignedQuantity <= 0 || !Precision(input.AssignedQuantity) ||
        Blank(input.ScopeDescription, 2000);

    private static async Task<IResult?> ValidateReferralAssignment(HelpingHandsDbContext database,
        AssistanceRequest request, ReferralInput input, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        if (request.IsMultiNeed) return Conflict("Multi-need referral is not available yet.");
        if (request.Version != input.Version || request.Status is not ("Accepted" or "Open") || request.RequiredQuantity is null)
            return Conflict("Request changed or is not eligible for referral.");
        if (request.Unit == "scope" && input.AssignedQuantity != 1) return Bad("Service scopes are indivisible.");
        var recipient = await database.Recipients
            .FromSqlInterpolated($"SELECT * FROM \"ReferralRecipient\" WHERE \"Id\" = {input.RecipientId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (recipient is null || recipient.RecipientType != "Charity" || recipient.ApprovalStatus != "Approved" || !recipient.Active)
            return Conflict("Recipient is not eligible.");

        var capacityError = await ValidateCharityDailyCapacityFreshnessAsync(database, request, input, timeProvider, cancellationToken);
        if (capacityError is not null)
            return capacityError;

        var assigned = await database.Referrals.Where(item => item.RequestId == request.Id)
            .SumAsync(item => (decimal?)item.AssignedQuantity, cancellationToken) ?? 0;
        return assigned + input.AssignedQuantity > request.RequiredQuantity
            ? Conflict("Quantity exceeds available need.") : null;
    }

    private static async Task<IResult?> ValidateCharityDailyCapacityFreshnessAsync(
        HelpingHandsDbContext database,
        AssistanceRequest request,
        ReferralInput input,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var enabledServices = await (
            from cs in database.CharityServices.AsNoTracking()
            join sv in database.ServiceVersions.AsNoTracking() on cs.ServiceVersionId equals sv.Id
            join stv in database.StandardVersions.AsNoTracking() on cs.StandardVersionId equals stv.Id
            join us in database.UnitStandards.AsNoTracking() on stv.UnitStandardId equals us.Id
            where cs.CharityId == input.RecipientId && cs.Enabled
            select new
            {
                cs.CharityServiceId,
                cs.ServiceVersionId,
                cs.StandardVersionId,
                ServiceName = sv.Name,
                UnitName = us.Name
            }
        ).ToListAsync(cancellationToken);

        Guid? targetCharityServiceId = null;

        if (input.ServiceVersionId.HasValue || input.StandardVersionId.HasValue)
        {
            if (!input.ServiceVersionId.HasValue || !input.StandardVersionId.HasValue)
                return Bad("Both serviceVersionId and standardVersionId must be provided together.");

            var exactMatch = enabledServices.SingleOrDefault(s =>
                s.ServiceVersionId == input.ServiceVersionId.Value &&
                s.StandardVersionId == input.StandardVersionId.Value);

            if (exactMatch is null)
                return Conflict("Recipient does not support the requested service version.");

            targetCharityServiceId = exactMatch.CharityServiceId;
        }
        else if (enabledServices.Count > 0 && !string.IsNullOrWhiteSpace(request.ServiceSubtype))
        {
            var matchedByName = enabledServices.FirstOrDefault(s =>
                string.Equals(s.ServiceName, request.ServiceSubtype, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrWhiteSpace(request.Unit) || string.Equals(s.UnitName, request.Unit, StringComparison.OrdinalIgnoreCase)));

            if (matchedByName is null)
                return Conflict("Recipient does not support the requested service version.");

            targetCharityServiceId = matchedByName.CharityServiceId;
        }

        if (targetCharityServiceId is null)
            return null;

        var todayCairo = GetCurrentCairoDate(timeProvider.GetUtcNow());
        var todayCapacity = await database.DailyCapacities.AsNoTracking()
            .SingleOrDefaultAsync(
                dc => dc.CharityServiceId == targetCharityServiceId.Value && dc.CairoDate == todayCairo,
                cancellationToken);

        if (todayCapacity is null)
            return Conflict("Daily capacity for this service version is stale and frozen until updated today (Cairo time).");

        return null;
    }
}