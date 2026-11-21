using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Referrals;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    public sealed record CharityCapacityItemInput(Guid CharityServiceId, decimal Available);

    public sealed record UpdateCharityCapacityInput(IReadOnlyList<CharityCapacityItemInput>? Capacities);

    public sealed record CharityDailyCapacityView(
        Guid? CapacityId,
        Guid CharityServiceId,
        Guid ServiceVersionId,
        string ServiceName,
        int ServiceVersionNumber,
        Guid StandardVersionId,
        string UnitStandardName,
        int StandardVersionNumber,
        DateOnly CairoDate,
        string Status,
        bool IsStale,
        decimal? Available,
        DateTimeOffset? UpdatedAt,
        DateOnly? LastUpdatedCairoDate,
        decimal? LastKnownAvailable);

    private static readonly TimeZoneInfo CairoTimeZone = ResolveCairoTimeZone();

    private static TimeZoneInfo ResolveCairoTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        }
    }

    private static DateOnly GetCurrentCairoDate(DateTimeOffset utcNow)
    {
        var cairoTime = TimeZoneInfo.ConvertTime(utcNow, CairoTimeZone);
        return DateOnly.FromDateTime(cairoTime.DateTime);
    }

    private static void MapRecipientCapacityRoutes(this RouteGroupBuilder api)
    {
        api.MapGet("/recipients/{id:guid}/capacity", GetCharityDailyCapacity).RequireAuthorization();
        api.MapPut("/recipients/{id:guid}/capacity", UpdateCharityDailyCapacity).RequireAuthorization();
    }

    private static async Task<IResult> GetCharityDailyCapacity(
        Guid id,
        ClaimsPrincipal user,
        HelpingHandsDbContext database,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var userId = Actor(user);
        if (!Staff(user) && !await Member(database, id, userId, cancellationToken))
            return Results.NotFound();

        var charityExists = await database.Charities.AsNoTracking()
            .AnyAsync(item => item.RecipientId == id, cancellationToken);
        if (!charityExists)
            return Results.NotFound();

        var cairoDate = GetCurrentCairoDate(timeProvider.GetUtcNow());
        var capacities = await BuildDailyCapacityViewAsync(database, id, cairoDate, cancellationToken);

        return Results.Ok(new
        {
            RecipientId = id,
            CairoDate = cairoDate,
            Capacities = capacities
        });
    }

    private static async Task<IResult> UpdateCharityDailyCapacity(
        Guid id,
        UpdateCharityCapacityInput input,
        ClaimsPrincipal user,
        HelpingHandsDbContext database,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!user.IsInRole("RecipientRepresentative"))
            return Results.Forbid();

        var userId = Actor(user);
        if (!await Member(database, id, userId, cancellationToken))
            return Results.NotFound();

        if (input.Capacities is null || input.Capacities.Count == 0)
            return Bad("At least one capacity update item is required.");

        var distinctIds = input.Capacities
            .Select(item => item.CharityServiceId)
            .Distinct()
            .ToList();

        if (distinctIds.Count != input.Capacities.Count)
            return Bad("Duplicate charity service entries are not allowed in a single update.");

        foreach (var item in input.Capacities)
        {
            if (item.Available < 0)
                return Bad("Available capacity quantity must be nonnegative.");

            if (decimal.Round(item.Available, 3) != item.Available)
                return Bad("Available capacity quantity supports at most 3 decimal places.");
        }

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        var charity = await PostgresAllocationLockProtocol.LockCharitiesOrderedAsync(
            database, [id], cancellationToken);
        if (charity.Count == 0)
            return Results.NotFound();

        await PostgresAllocationLockProtocol.LockCharityServicesOrderedAsync(
            database, distinctIds, cancellationToken);

        var enabledServices = await database.CharityServices.AsNoTracking()
            .Where(cs => cs.CharityId == id && cs.Enabled && distinctIds.Contains(cs.CharityServiceId))
            .Select(cs => cs.CharityServiceId)
            .ToListAsync(cancellationToken);

        if (enabledServices.Count != distinctIds.Count)
            return Bad("All capacity updates must target enabled services belonging to this charity.");

        var now = timeProvider.GetUtcNow();
        var cairoDate = GetCurrentCairoDate(now);

        var existingCapacityIds = await database.DailyCapacities
            .Where(dc => distinctIds.Contains(dc.CharityServiceId) && dc.CairoDate == cairoDate)
            .Select(dc => dc.CapacityId)
            .ToListAsync(cancellationToken);

        await PostgresAllocationLockProtocol.LockDailyCapacitiesOrderedAsync(
            database, existingCapacityIds, cancellationToken);

        var existingCapacities = await database.DailyCapacities
            .Where(dc => distinctIds.Contains(dc.CharityServiceId) && dc.CairoDate == cairoDate)
            .ToDictionaryAsync(dc => dc.CharityServiceId, cancellationToken);

        foreach (var item in input.Capacities)
        {
            if (existingCapacities.TryGetValue(item.CharityServiceId, out var existing))
            {
                existing.Available = item.Available;
                existing.UpdatedAt = now;

                database.CapacityUpdates.Add(new CapacityUpdate
                {
                    CapacityId = existing.CapacityId,
                    ActorId = userId,
                    Available = item.Available,
                    UpdatedAt = now
                });
            }
            else
            {
                var capacity = new DailyCapacity
                {
                    CharityServiceId = item.CharityServiceId,
                    CairoDate = cairoDate,
                    Available = item.Available,
                    UpdatedAt = now
                };

                database.DailyCapacities.Add(capacity);
                database.CapacityUpdates.Add(new CapacityUpdate
                {
                    CapacityId = capacity.CapacityId,
                    ActorId = userId,
                    Available = item.Available,
                    UpdatedAt = now
                });
            }
        }

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var updatedCapacities = await BuildDailyCapacityViewAsync(database, id, cairoDate, cancellationToken);

        return Results.Ok(new
        {
            RecipientId = id,
            CairoDate = cairoDate,
            Capacities = updatedCapacities
        });
    }

    private static async Task<IReadOnlyList<CharityDailyCapacityView>> BuildDailyCapacityViewAsync(
        HelpingHandsDbContext database,
        Guid recipientId,
        DateOnly cairoDate,
        CancellationToken cancellationToken)
    {
        var services = await (
            from cs in database.CharityServices.AsNoTracking()
            join sv in database.ServiceVersions.AsNoTracking() on cs.ServiceVersionId equals sv.Id
            join stv in database.StandardVersions.AsNoTracking() on cs.StandardVersionId equals stv.Id
            join us in database.UnitStandards.AsNoTracking() on stv.UnitStandardId equals us.Id
            where cs.CharityId == recipientId && cs.Enabled
            orderby sv.Name, sv.VersionNumber, us.Name, stv.VersionNumber
            select new
            {
                cs.CharityServiceId,
                ServiceVersionId = sv.Id,
                ServiceName = sv.Name,
                ServiceVersionNumber = sv.VersionNumber,
                StandardVersionId = stv.Id,
                UnitStandardName = us.Name,
                StandardVersionNumber = stv.VersionNumber
            }
        ).ToListAsync(cancellationToken);

        if (services.Count == 0)
            return [];

        var serviceIds = services.Select(s => s.CharityServiceId).ToList();

        var latestCapacities = await database.DailyCapacities.AsNoTracking()
            .Where(dc => serviceIds.Contains(dc.CharityServiceId) && dc.CairoDate <= cairoDate)
            .OrderByDescending(dc => dc.CairoDate)
            .ToListAsync(cancellationToken);

        var latestByService = latestCapacities
            .GroupBy(dc => dc.CharityServiceId)
            .ToDictionary(g => g.Key, g => g.First());

        var result = new List<CharityDailyCapacityView>(services.Count);
        foreach (var s in services)
        {
            if (latestByService.TryGetValue(s.CharityServiceId, out var latest) && latest.CairoDate == cairoDate)
            {
                var status = latest.Available == 0 ? "Zero" : "Available";
                result.Add(new CharityDailyCapacityView(
                    latest.CapacityId,
                    s.CharityServiceId,
                    s.ServiceVersionId,
                    s.ServiceName,
                    s.ServiceVersionNumber,
                    s.StandardVersionId,
                    s.UnitStandardName,
                    s.StandardVersionNumber,
                    cairoDate,
                    status,
                    IsStale: false,
                    Available: latest.Available,
                    UpdatedAt: latest.UpdatedAt,
                    LastUpdatedCairoDate: latest.CairoDate,
                    LastKnownAvailable: latest.Available));
            }
            else
            {
                result.Add(new CharityDailyCapacityView(
                    CapacityId: null,
                    s.CharityServiceId,
                    s.ServiceVersionId,
                    s.ServiceName,
                    s.ServiceVersionNumber,
                    s.StandardVersionId,
                    s.UnitStandardName,
                    s.StandardVersionNumber,
                    cairoDate,
                    Status: "Stale",
                    IsStale: true,
                    Available: null,
                    UpdatedAt: null,
                    LastUpdatedCairoDate: latest?.CairoDate,
                    LastKnownAvailable: latest?.Available));
            }
        }

        return result;
    }
}
