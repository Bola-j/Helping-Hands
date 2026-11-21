using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Referrals;

/// <summary>
/// Shared ordered PostgreSQL row-lock and transaction protocol for capacity updates and referral allocations.
/// Global lock acquisition order (always ascending by key within each tier):
/// 1. AssistanceRequest ("AssistanceRequest".Id)
/// 2. Recipient / Charity ("ReferralRecipient".Id, then "Charity".RecipientId)
/// 3. CharityService ("CharityService".CharityServiceId)
/// 4. DailyCapacity ("DailyCapacity".CapacityId)
/// </summary>
public static class PostgresAllocationLockProtocol
{
    public const int TierAssistanceRequest = 1;
    public const int TierRecipientAndCharity = 2;
    public const int TierCharityService = 3;
    public const int TierDailyCapacity = 4;

    public static IReadOnlyList<string> OrderedLockTiers { get; } =
    [
        "1. AssistanceRequest (ordered by Id)",
        "2. ReferralRecipient / Charity (ordered by Id / RecipientId)",
        "3. CharityService (ordered by CharityServiceId)",
        "4. DailyCapacity (ordered by CapacityId)"
    ];

    public static IReadOnlyList<Guid> OrderIdsDeterministically(IEnumerable<Guid> ids) =>
        ids.Distinct().OrderBy(id => id).ToArray();

    public static async Task<IReadOnlyList<AssistanceRequest>> LockRequestsOrderedAsync(
        HelpingHandsDbContext database,
        IEnumerable<Guid> requestIds,
        CancellationToken cancellationToken = default)
    {
        var orderedIds = OrderIdsDeterministically(requestIds);
        if (orderedIds.Count == 0)
            return [];

        var locked = new List<AssistanceRequest>(orderedIds.Count);
        foreach (var id in orderedIds)
        {
            var row = await database.Requests
                .FromSqlInterpolated($"SELECT * FROM \"AssistanceRequest\" WHERE \"Id\" = {id} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);
            if (row is not null)
                locked.Add(row);
        }

        return locked;
    }

    public static async Task<IReadOnlyList<Recipient>> LockRecipientsOrderedAsync(
        HelpingHandsDbContext database,
        IEnumerable<Guid> recipientIds,
        CancellationToken cancellationToken = default)
    {
        var orderedIds = OrderIdsDeterministically(recipientIds);
        if (orderedIds.Count == 0)
            return [];

        var locked = new List<Recipient>(orderedIds.Count);
        foreach (var id in orderedIds)
        {
            var row = await database.Recipients
                .FromSqlInterpolated($"SELECT * FROM \"ReferralRecipient\" WHERE \"Id\" = {id} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);
            if (row is not null)
                locked.Add(row);
        }

        return locked;
    }

    public static async Task<IReadOnlyList<Charity>> LockCharitiesOrderedAsync(
        HelpingHandsDbContext database,
        IEnumerable<Guid> charityIds,
        CancellationToken cancellationToken = default)
    {
        var orderedIds = OrderIdsDeterministically(charityIds);
        if (orderedIds.Count == 0)
            return [];

        var locked = new List<Charity>(orderedIds.Count);
        foreach (var id in orderedIds)
        {
            var row = await database.Charities
                .FromSqlInterpolated($"SELECT * FROM \"Charity\" WHERE \"RecipientId\" = {id} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);
            if (row is not null)
                locked.Add(row);
        }

        return locked;
    }

    public static async Task<IReadOnlyList<CharityService>> LockCharityServicesOrderedAsync(
        HelpingHandsDbContext database,
        IEnumerable<Guid> charityServiceIds,
        CancellationToken cancellationToken = default)
    {
        var orderedIds = OrderIdsDeterministically(charityServiceIds);
        if (orderedIds.Count == 0)
            return [];

        var locked = new List<CharityService>(orderedIds.Count);
        foreach (var id in orderedIds)
        {
            var row = await database.CharityServices
                .FromSqlInterpolated($"SELECT * FROM \"CharityService\" WHERE \"CharityServiceId\" = {id} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);
            if (row is not null)
                locked.Add(row);
        }

        return locked;
    }

    public static async Task<IReadOnlyList<DailyCapacity>> LockDailyCapacitiesOrderedAsync(
        HelpingHandsDbContext database,
        IEnumerable<Guid> capacityIds,
        CancellationToken cancellationToken = default)
    {
        var orderedIds = OrderIdsDeterministically(capacityIds);
        if (orderedIds.Count == 0)
            return [];

        var locked = new List<DailyCapacity>(orderedIds.Count);
        foreach (var id in orderedIds)
        {
            var row = await database.DailyCapacities
                .FromSqlInterpolated($"SELECT * FROM \"DailyCapacity\" WHERE \"CapacityId\" = {id} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken);
            if (row is not null)
                locked.Add(row);
        }

        return locked;
    }

    public static async Task AcquireOrderedLocksAsync(
        HelpingHandsDbContext database,
        IEnumerable<Guid>? requestIds = null,
        IEnumerable<Guid>? recipientOrCharityIds = null,
        IEnumerable<Guid>? charityServiceIds = null,
        IEnumerable<Guid>? capacityIds = null,
        CancellationToken cancellationToken = default)
    {
        // Tier 1: AssistanceRequest
        if (requestIds is not null)
            await LockRequestsOrderedAsync(database, requestIds, cancellationToken);

        // Tier 2: Recipient then Charity
        if (recipientOrCharityIds is not null)
        {
            var orderedRecipients = OrderIdsDeterministically(recipientOrCharityIds);
            await LockRecipientsOrderedAsync(database, orderedRecipients, cancellationToken);
            await LockCharitiesOrderedAsync(database, orderedRecipients, cancellationToken);
        }

        // Tier 3: CharityService
        if (charityServiceIds is not null)
            await LockCharityServicesOrderedAsync(database, charityServiceIds, cancellationToken);

        // Tier 4: DailyCapacity (after service/capacity target resolution)
        if (capacityIds is not null)
            await LockDailyCapacitiesOrderedAsync(database, capacityIds, cancellationToken);
    }
}
