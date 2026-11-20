using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    public sealed record EligibleCharityCandidateView(
        Guid RecipientId,
        string CharityName,
        string? RegistrationInformation,
        Guid CharityServiceId,
        Guid ServiceCategoryId,
        string ServiceCategoryCode,
        Guid ServiceVersionId,
        string ServiceName,
        int ServiceVersionNumber,
        Guid StandardVersionId,
        string UnitStandardName,
        int StandardVersionNumber,
        DateOnly CairoDate,
        bool IsFreshToday,
        bool IsStale,
        string CapacityStatus,
        decimal AvailableCapacity,
        DateTimeOffset CapacityUpdatedAt);

    public sealed record EligibleCharityMatchResponse(
        DateOnly CairoDate,
        Guid? ServiceCategoryId,
        Guid? ServiceVersionId,
        Guid? StandardVersionId,
        int MatchCount,
        IReadOnlyList<EligibleCharityCandidateView> Candidates);

    private static void MapEligibleCharityMatchingRoute(this RouteGroupBuilder api)
    {
        api.MapGet("/matching/charities", GetEligibleCharities).RequireAuthorization();
    }

    private static async Task<IResult> GetEligibleCharities(
        Guid? serviceCategoryId,
        Guid? serviceVersionId,
        Guid? standardVersionId,
        Guid? requestId,
        Guid? needId,
        decimal? minAvailable,
        ClaimsPrincipal user,
        HelpingHandsDbContext database,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!Coordinator(user))
            return Results.Forbid();

        if (minAvailable is < 0)
            return Bad("minAvailable must be nonnegative.");

        var resolvedCategoryId = serviceCategoryId;
        var resolvedServiceVersionId = serviceVersionId;
        var resolvedStandardVersionId = standardVersionId;

        if (needId.HasValue)
        {
            var need = await database.RequestNeeds.AsNoTracking()
                .SingleOrDefaultAsync(n => n.Id == needId.Value, cancellationToken);
            if (need is null)
                return Results.NotFound();

            if (requestId.HasValue && need.RequestId != requestId.Value)
                return Bad("Specified needId does not belong to requestId.");

            resolvedStandardVersionId ??= need.StandardVersionId;
        }

        if (requestId.HasValue)
        {
            var requestExists = await database.Requests.AsNoTracking()
                .AnyAsync(r => r.Id == requestId.Value, cancellationToken);
            if (!requestExists)
                return Results.NotFound();
        }

        if (!resolvedCategoryId.HasValue || !resolvedServiceVersionId.HasValue || !resolvedStandardVersionId.HasValue)
            return Bad("serviceCategoryId, serviceVersionId, and standardVersionId are required (or derivable from a versioned need).");

        var cairoDate = GetCurrentCairoDate(timeProvider.GetUtcNow());

        // Exact version matching only: no silent conversion from older/newer service or standard versions.
        var rows = await (
            from cs in database.CharityServices.AsNoTracking()
            join r in database.Recipients.AsNoTracking() on cs.CharityId equals r.Id
            join c in database.Charities.AsNoTracking() on cs.CharityId equals c.RecipientId
            join sv in database.ServiceVersions.AsNoTracking() on cs.ServiceVersionId equals sv.Id
            join sc in database.ServiceCategories.AsNoTracking() on sv.CategoryId equals sc.Id
            join stv in database.StandardVersions.AsNoTracking() on cs.StandardVersionId equals stv.Id
            join us in database.UnitStandards.AsNoTracking() on stv.UnitStandardId equals us.Id
            join dc in database.DailyCapacities.AsNoTracking()
                on new { cs.CharityServiceId, CairoDate = cairoDate }
                equals new { dc.CharityServiceId, dc.CairoDate } into capacities
            from todayCap in capacities.DefaultIfEmpty()
            where cs.Enabled
                && r.RecipientType == "Charity"
                && r.ApprovalStatus == "Approved"
                && r.Active
                && sv.CategoryId == resolvedCategoryId.Value
                && cs.ServiceVersionId == resolvedServiceVersionId.Value
                && cs.StandardVersionId == resolvedStandardVersionId.Value
            select new
            {
                RecipientId = r.Id,
                CharityName = r.DisplayName,
                c.RegistrationInformation,
                cs.CharityServiceId,
                ServiceCategoryId = sc.Id,
                ServiceCategoryCode = sc.Code,
                ServiceVersionId = sv.Id,
                ServiceName = sv.Name,
                ServiceVersionNumber = sv.VersionNumber,
                StandardVersionId = stv.Id,
                UnitStandardName = us.Name,
                StandardVersionNumber = stv.VersionNumber,
                HasFreshCapacity = todayCap != null,
                Available = todayCap != null ? todayCap.Available : 0m,
                UpdatedAt = todayCap != null ? todayCap.UpdatedAt : default
            }
        ).ToListAsync(cancellationToken);

        var requiredFloor = minAvailable.HasValue && minAvailable.Value > 0
            ? minAvailable.Value
            : 0.0000000001m;

        var candidates = rows
            .Where(x => x.HasFreshCapacity && x.Available >= requiredFloor)
            .OrderByDescending(x => x.Available)
            .ThenBy(x => x.CharityName)
            .Select(x => new EligibleCharityCandidateView(
                RecipientId: x.RecipientId,
                CharityName: x.CharityName,
                RegistrationInformation: x.RegistrationInformation,
                CharityServiceId: x.CharityServiceId,
                ServiceCategoryId: x.ServiceCategoryId,
                ServiceCategoryCode: x.ServiceCategoryCode,
                ServiceVersionId: x.ServiceVersionId,
                ServiceName: x.ServiceName,
                ServiceVersionNumber: x.ServiceVersionNumber,
                StandardVersionId: x.StandardVersionId,
                UnitStandardName: x.UnitStandardName,
                StandardVersionNumber: x.StandardVersionNumber,
                CairoDate: cairoDate,
                IsFreshToday: true,
                IsStale: false,
                CapacityStatus: "Available",
                AvailableCapacity: x.Available,
                CapacityUpdatedAt: x.UpdatedAt))
            .ToList();

        return Results.Ok(new EligibleCharityMatchResponse(
            CairoDate: cairoDate,
            ServiceCategoryId: resolvedCategoryId.Value,
            ServiceVersionId: resolvedServiceVersionId.Value,
            StandardVersionId: resolvedStandardVersionId.Value,
            MatchCount: candidates.Count,
            Candidates: candidates));
    }
}