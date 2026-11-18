using System.Security.Claims;
using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    public sealed record CharityServiceSelectionInput(Guid ServiceVersionId, Guid StandardVersionId);

    public sealed record UpdateCharityServicesInput(IReadOnlyList<CharityServiceSelectionInput>? Services);

    public sealed record CharitySelectedServiceView(
        Guid CharityServiceId,
        Guid CategoryId,
        string CategoryCode,
        Guid ServiceVersionId,
        string ServiceName,
        int ServiceVersionNumber,
        Guid UnitStandardId,
        string UnitStandardName,
        Guid StandardVersionId,
        int StandardVersionNumber,
        DateTimeOffset EffectiveAt,
        bool Enabled);

    public sealed record ApplicablePlatformUnitOptionView(
        Guid CategoryId,
        string CategoryCode,
        Guid ServiceVersionId,
        string ServiceName,
        int ServiceVersionNumber,
        Guid UnitStandardId,
        string UnitStandardName,
        Guid StandardVersionId,
        int StandardVersionNumber,
        DateTimeOffset EffectiveAt);

    private static void MapRecipientServiceRoutes(this RouteGroupBuilder api)
    {
        api.MapGet("/recipients/{id:guid}/services", GetCharityServices).RequireAuthorization();
        api.MapPut("/recipients/{id:guid}/services", UpdateCharityServices).RequireAuthorization();
    }

    private static async Task<IResult> GetCharityServices(
        Guid id,
        ClaimsPrincipal user,
        HelpingHandsDbContext database,
        CancellationToken cancellationToken)
    {
        var userId = Actor(user);
        if (!Staff(user) && !await Member(database, id, userId, cancellationToken))
            return Results.NotFound();

        var charityExists = await database.Charities.AsNoTracking()
            .AnyAsync(item => item.RecipientId == id, cancellationToken);
        if (!charityExists)
            return Results.NotFound();

        var selectedServices = await BuildSelectedServicesViewAsync(database, id, cancellationToken);
        var applicableOptions = await BuildApplicableOptionsViewAsync(database, cancellationToken);

        return Results.Ok(new
        {
            RecipientId = id,
            SelectedServices = selectedServices,
            ApplicableOptions = applicableOptions
        });
    }

    private static async Task<IResult> UpdateCharityServices(
        Guid id,
        UpdateCharityServicesInput input,
        ClaimsPrincipal user,
        HelpingHandsDbContext database,
        CancellationToken cancellationToken)
    {
        if (!user.IsInRole("RecipientRepresentative"))
            return Results.Forbid();

        var userId = Actor(user);
        if (!await Member(database, id, userId, cancellationToken))
            return Results.NotFound();

        if (input.Services is null)
            return Bad("Services selection list is required.");

        var distinctPairs = input.Services
            .Select(item => (item.ServiceVersionId, item.StandardVersionId))
            .Distinct()
            .ToList();

        if (distinctPairs.Count != input.Services.Count)
            return Bad("Duplicate service and platform-unit version selections are not allowed.");

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        var charity = await database.Charities
            .FromSqlInterpolated($"SELECT * FROM \"Charity\" WHERE \"RecipientId\" = {id} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (charity is null)
            return Results.NotFound();

        if (distinctPairs.Count > 0)
        {
            var serviceIds = distinctPairs.Select(x => x.ServiceVersionId).Distinct().ToArray();
            var standardVersionIds = distinctPairs.Select(x => x.StandardVersionId).Distinct().ToArray();

            var publishedServices = await database.ServiceVersions.AsNoTracking()
                .Where(s => serviceIds.Contains(s.Id) && s.IsPublished)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            if (publishedServices.Count != serviceIds.Length)
                return Bad("All selected services must exist and be published.");

            var validStandardMappings = await (
                from sv in database.StandardVersions.AsNoTracking()
                join us in database.UnitStandards.AsNoTracking() on sv.UnitStandardId equals us.Id
                where standardVersionIds.Contains(sv.Id) && sv.IsPublished
                select new { StandardVersionId = sv.Id, us.ServiceVersionId }
            ).ToListAsync(cancellationToken);

            var standardToService = validStandardMappings.ToDictionary(x => x.StandardVersionId, x => x.ServiceVersionId);

            foreach (var (serviceVersionId, standardVersionId) in distinctPairs)
            {
                if (!standardToService.TryGetValue(standardVersionId, out var ownerServiceVersionId)
                    || ownerServiceVersionId != serviceVersionId)
                {
                    return Bad("Each selected platform-unit version must be published and belong to its paired service version.");
                }
            }
        }

        var existingRows = await database.CharityServices
            .Where(cs => cs.CharityId == id)
            .ToListAsync(cancellationToken);

        var selectedSet = distinctPairs.ToHashSet();

        foreach (var existing in existingRows)
        {
            existing.Enabled = selectedSet.Contains((existing.ServiceVersionId, existing.StandardVersionId));
        }

        var existingSet = existingRows
            .Select(cs => (cs.ServiceVersionId, cs.StandardVersionId))
            .ToHashSet();

        foreach (var (serviceVersionId, standardVersionId) in distinctPairs)
        {
            if (!existingSet.Contains((serviceVersionId, standardVersionId)))
            {
                database.CharityServices.Add(new CharityService
                {
                    CharityId = id,
                    ServiceVersionId = serviceVersionId,
                    StandardVersionId = standardVersionId,
                    Enabled = true
                });
            }
        }

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var updatedServices = await BuildSelectedServicesViewAsync(database, id, cancellationToken);
        return Results.Ok(new
        {
            RecipientId = id,
            SelectedServices = updatedServices
        });
    }

    private static async Task<IReadOnlyList<CharitySelectedServiceView>> BuildSelectedServicesViewAsync(
        HelpingHandsDbContext database,
        Guid recipientId,
        CancellationToken cancellationToken)
    {
        return await (
            from cs in database.CharityServices.AsNoTracking()
            join sv in database.ServiceVersions.AsNoTracking() on cs.ServiceVersionId equals sv.Id
            join cat in database.ServiceCategories.AsNoTracking() on sv.CategoryId equals cat.Id
            join stv in database.StandardVersions.AsNoTracking() on cs.StandardVersionId equals stv.Id
            join us in database.UnitStandards.AsNoTracking() on stv.UnitStandardId equals us.Id
            where cs.CharityId == recipientId && cs.Enabled
            orderby cat.Code, sv.Name, sv.VersionNumber, stv.VersionNumber
            select new CharitySelectedServiceView(
                cs.CharityServiceId,
                cat.Id,
                cat.Code,
                sv.Id,
                sv.Name,
                sv.VersionNumber,
                us.Id,
                us.Name,
                stv.Id,
                stv.VersionNumber,
                stv.EffectiveAt,
                cs.Enabled)
        ).ToListAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<ApplicablePlatformUnitOptionView>> BuildApplicableOptionsViewAsync(
        HelpingHandsDbContext database,
        CancellationToken cancellationToken)
    {
        return await (
            from cat in database.ServiceCategories.AsNoTracking()
            join sv in database.ServiceVersions.AsNoTracking() on cat.Id equals sv.CategoryId
            join us in database.UnitStandards.AsNoTracking() on sv.Id equals us.ServiceVersionId
            join stv in database.StandardVersions.AsNoTracking() on us.Id equals stv.UnitStandardId
            where sv.IsPublished && stv.IsPublished
            orderby cat.Code, sv.Name, sv.VersionNumber, us.Name, stv.VersionNumber
            select new ApplicablePlatformUnitOptionView(
                cat.Id,
                cat.Code,
                sv.Id,
                sv.Name,
                sv.VersionNumber,
                us.Id,
                us.Name,
                stv.Id,
                stv.VersionNumber,
                stv.EffectiveAt)
        ).ToListAsync(cancellationToken);
    }
}