using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Reminders;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Referrals;

public sealed record SplitAllocationLegInput(
    Guid RecipientId,
    Guid ServiceVersionId,
    Guid StandardVersionId,
    decimal Quantity,
    string ScopeDescription);

public sealed record SplitReferralAllocationRequest(
    Guid RequestId,
    Guid? NeedId,
    Guid ActorUserId,
    IReadOnlyList<SplitAllocationLegInput> Allocations);

public sealed record SplitAllocationLegOutcome(
    Guid RecipientId,
    Guid CharityServiceId,
    Guid ServiceVersionId,
    Guid StandardVersionId,
    decimal AllocatedQuantity,
    decimal RemainingCharityCapacity,
    string ReferralReference);

public sealed record SplitReferralAllocationResult(
    bool Succeeded,
    string Status,
    string? ErrorCode,
    string? ErrorMessage,
    Guid RequestId,
    Guid? NeedId,
    string RequestStatus,
    string? NeedStatus,
    decimal TotalAllocatedQuantity,
    decimal RemainingRequestQuantity,
    bool LeaveOpen,
    bool SilentConversionApplied,
    IReadOnlyList<SplitAllocationLegOutcome> Legs);

public sealed partial class SplitReferralAllocationService(
    HelpingHandsDbContext database,
    TimeProvider timeProvider)
{
    public async Task<SplitReferralAllocationResult> AllocateSplitAsync(
        SplitReferralAllocationRequest command,
        CancellationToken cancellationToken = default)
    {
        if (command.Allocations is null || command.Allocations.Count == 0)
        {
            return Failure("InvalidInput", "At least one allocation leg is required.", command.RequestId, command.NeedId, "Open", null, 0m, 0m, leaveOpen: true);
        }

        foreach (var leg in command.Allocations)
        {
            if (leg.Quantity <= 0m || decimal.Round(leg.Quantity, 3) != leg.Quantity)
            {
                return Failure("InvalidQuantity", "Each allocation leg must have a positive quantity with at most 3 decimal places.", command.RequestId, command.NeedId, "Open", null, 0m, 0m, leaveOpen: true);
            }

            if (string.IsNullOrWhiteSpace(leg.ScopeDescription) || leg.ScopeDescription.Trim().Length > 2000)
            {
                return Failure("InvalidScope", "Scope description is required and must be at most 2000 characters.", command.RequestId, command.NeedId, "Open", null, 0m, 0m, leaveOpen: true);
            }
        }

        var cairoDate = DailyReminderStateService.GetCairoDate(timeProvider.GetUtcNow());
        var now = timeProvider.GetUtcNow();

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        // Tier 1 lock: AssistanceRequest
        var lockedRequests = await PostgresAllocationLockProtocol.LockRequestsOrderedAsync(
            database,
            [command.RequestId],
            cancellationToken);

        var request = lockedRequests.SingleOrDefault(r => r.Id == command.RequestId);
        if (request is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure("RequestNotFound", "Assistance request was not found.", command.RequestId, command.NeedId, "Open", null, 0m, 0m, leaveOpen: true);
        }

        RequestNeed? need = null;
        if (command.NeedId.HasValue)
        {
            need = await database.RequestNeeds
                .SingleOrDefaultAsync(n => n.Id == command.NeedId.Value && n.RequestId == request.Id, cancellationToken);

            if (need is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure("NeedNotFound", "Specified need does not belong to the assistance request.", request.Id, command.NeedId, request.Status, null, 0m, 0m, leaveOpen: true);
            }
        }

        var targetQuantity = need?.Quantity ?? request.RequiredQuantity;
        if (targetQuantity is null || targetQuantity <= 0m)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure("MissingTargetQuantity", "Target quantity is not specified for this request or need.", request.Id, need?.Id, request.Status, need?.Status, 0m, 0m, leaveOpen: true);
        }

        var requestedTotal = command.Allocations.Sum(a => a.Quantity);
        var requestReferrals = database.Referrals.Where(refItem => refItem.RequestId == request.Id);
        requestReferrals = need is null ? requestReferrals.Where(refItem => refItem.NeedId == null) :
            requestReferrals.Where(refItem => refItem.NeedId == need.Id);
        var alreadyAssigned = await requestReferrals.SumAsync(refItem => (decimal?)refItem.AssignedQuantity, cancellationToken) ?? 0m;

        var remainingBeforeAllocation = targetQuantity.Value - alreadyAssigned;
        if (requestedTotal > remainingBeforeAllocation)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(
                "OverAllocation",
                "Total split allocation exceeds remaining unallocated need quantity.",
                request.Id,
                need?.Id,
                "Open",
                need?.Status ?? "Open",
                alreadyAssigned,
                remainingBeforeAllocation,
                leaveOpen: true);
        }

        // Tier 2 locks: Recipient then Charity in deterministic ascending order
        var orderedRecipientIds = PostgresAllocationLockProtocol.OrderIdsDeterministically(
            command.Allocations.Select(a => a.RecipientId));

        await PostgresAllocationLockProtocol.LockRecipientsOrderedAsync(database, orderedRecipientIds, cancellationToken);
        await PostgresAllocationLockProtocol.LockCharitiesOrderedAsync(database, orderedRecipientIds, cancellationToken);

        var eligibleRecipients = await (
            from r in database.Recipients
            join c in database.Charities on r.Id equals c.RecipientId
            where orderedRecipientIds.Contains(r.Id)
                && r.RecipientType == "Charity"
                && r.ApprovalStatus == "Approved"
                && r.Active
            select r.Id
        ).ToListAsync(cancellationToken);

        var eligibleRecipientSet = eligibleRecipients.ToHashSet();
        if (eligibleRecipientSet.Count != orderedRecipientIds.Count)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Failure(
                "IneligibleRecipient",
                "One or more target charities are not approved or active; need remains open.",
                request.Id,
                need?.Id,
                "Open",
                need?.Status ?? "Open",
                alreadyAssigned,
                remainingBeforeAllocation,
                leaveOpen: true);
        }

        // Exact-version check: every leg must match an enabled CharityService with exact ServiceVersionId & StandardVersionId
        var matchedServicesByLeg = new Dictionary<int, CharityService>();
        for (var i = 0; i < command.Allocations.Count; i++)
        {
            var leg = command.Allocations[i];
            if (need is not null && leg.StandardVersionId != need.StandardVersionId)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure(
                    "ExactVersionMismatch",
                    "Allocation leg does not match the need's exact recorded standard version; no silent conversion is permitted.",
                    request.Id,
                    need.Id,
                    "Open",
                    "Open",
                    alreadyAssigned,
                    remainingBeforeAllocation,
                    leaveOpen: true);
            }

            var service = await database.CharityServices
                .SingleOrDefaultAsync(
                    cs => cs.CharityId == leg.RecipientId
                        && cs.ServiceVersionId == leg.ServiceVersionId
                        && cs.StandardVersionId == leg.StandardVersionId
                        && cs.Enabled,
                    cancellationToken);

            if (service is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure(
                    "ExactVersionUnmatched",
                    "No enabled charity service matches the exact requested service and standard versions; leaving need open.",
                    request.Id,
                    need?.Id,
                    "Open",
                    need?.Status ?? "Open",
                    alreadyAssigned,
                    remainingBeforeAllocation,
                    leaveOpen: true);
            }

            matchedServicesByLeg[i] = service;
        }

        var serviceIds = matchedServicesByLeg.Values.Select(s => s.CharityServiceId).Distinct().ToList();
        var todayCapacities = await database.DailyCapacities
            .Where(dc => serviceIds.Contains(dc.CharityServiceId) && dc.CairoDate == cairoDate)
            .ToListAsync(cancellationToken);

        var capacityByServiceId = todayCapacities.ToDictionary(dc => dc.CharityServiceId);

        // Ensure every matched service has fresh Cairo-date capacity before locking
        foreach (var service in matchedServicesByLeg.Values)
        {
            if (!capacityByServiceId.ContainsKey(service.CharityServiceId))
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure(
                    "StaleCapacity",
                    "Daily capacity for the exact service version is stale or missing for today's Cairo date; leaving need open.",
                    request.Id,
                    need?.Id,
                    "Open",
                    need?.Status ?? "Open",
                    alreadyAssigned,
                    remainingBeforeAllocation,
                    leaveOpen: true);
            }
        }

        // Tier 4 locks: DailyCapacity ordered deterministically by CapacityId
        var orderedCapacityIds = PostgresAllocationLockProtocol.OrderIdsDeterministically(
            capacityByServiceId.Values.Select(dc => dc.CapacityId));
        var lockedCapacities = await PostgresAllocationLockProtocol.LockDailyCapacitiesOrderedAsync(
            database,
            orderedCapacityIds,
            cancellationToken);
        var lockedCapacityById = lockedCapacities.ToDictionary(dc => dc.CapacityId);

        // Verify per-charity available capacity across legs (supporting multiple legs if needed)
        var projectedDeductionByCapacityId = new Dictionary<Guid, decimal>();
        for (var i = 0; i < command.Allocations.Count; i++)
        {
            var leg = command.Allocations[i];
            var service = matchedServicesByLeg[i];
            var cap = capacityByServiceId[service.CharityServiceId];
            var lockedCap = lockedCapacityById[cap.CapacityId];

            var currentDeduction = projectedDeductionByCapacityId.GetValueOrDefault(lockedCap.CapacityId, 0m);
            var nextDeduction = currentDeduction + leg.Quantity;

            if (lockedCap.Available - nextDeduction < 0m)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Failure(
                    "InsufficientDailyCapacity",
                    "Charity available daily capacity is insufficient for this split allocation leg.",
                    request.Id,
                    need?.Id,
                    "Open",
                    need?.Status ?? "Open",
                    alreadyAssigned,
                    remainingBeforeAllocation,
                    leaveOpen: true);
            }

            projectedDeductionByCapacityId[lockedCap.CapacityId] = nextDeduction;
        }

        var legOutcomes = new List<SplitAllocationLegOutcome>(command.Allocations.Count);
        for (var i = 0; i < command.Allocations.Count; i++)
        {
            var leg = command.Allocations[i];
            var service = matchedServicesByLeg[i];
            var cap = capacityByServiceId[service.CharityServiceId];
            var lockedCap = lockedCapacityById[cap.CapacityId];

            lockedCap.Available = decimal.Round(lockedCap.Available - leg.Quantity, 3);
            lockedCap.UpdatedAt = now;

            database.CapacityUpdates.Add(new CapacityUpdate
            {
                CapacityId = lockedCap.CapacityId,
                ActorId = command.ActorUserId,
                Available = lockedCap.Available,
                UpdatedAt = now
            });

            var referral = new Referral
            {
                RequestId = request.Id,
                NeedId = need?.Id,
                RecipientId = leg.RecipientId,
                AssignedQuantity = leg.Quantity,
                ScopeDescription = leg.ScopeDescription.Trim(),
                CreatedByUserId = command.ActorUserId,
                Reference = await database.NextReferenceAsync("REF", cancellationToken)
            };

            database.Referrals.Add(referral);
            database.DecisionHistory.Add(new DecisionHistory
            {
                ReferralId = referral.Id,
                RequestId = request.Id,
                ActorUserId = command.ActorUserId,
                Action = "SplitAllocated",
                OccurredAt = now
            });

            legOutcomes.Add(new SplitAllocationLegOutcome(
                RecipientId: leg.RecipientId,
                CharityServiceId: service.CharityServiceId,
                ServiceVersionId: leg.ServiceVersionId,
                StandardVersionId: leg.StandardVersionId,
                AllocatedQuantity: leg.Quantity,
                RemainingCharityCapacity: lockedCap.Available,
                ReferralReference: referral.Reference));
        }

        var totalAllocatedAfter = alreadyAssigned + requestedTotal;
        var remainingAfter = decimal.Round(targetQuantity.Value - totalAllocatedAfter, 3);

        request.Status = await RequestAllocationStatusAsync(database, request, need, remainingAfter, cancellationToken);
        request.Version = Guid.NewGuid();

        if (need is not null)
        {
            need.Status = remainingAfter == 0m ? "Allocated" : "Open";
        }

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new SplitReferralAllocationResult(
            Succeeded: true,
            Status: remainingAfter == 0m ? "Allocated" : "PartiallyAllocated",
            ErrorCode: null,
            ErrorMessage: null,
            RequestId: request.Id,
            NeedId: need?.Id,
            RequestStatus: request.Status,
            NeedStatus: need?.Status,
            TotalAllocatedQuantity: totalAllocatedAfter,
            RemainingRequestQuantity: remainingAfter,
            LeaveOpen: remainingAfter > 0m,
            SilentConversionApplied: false,
            Legs: legOutcomes);
    }

    private static SplitReferralAllocationResult Failure(
        string errorCode,
        string errorMessage,
        Guid requestId,
        Guid? needId,
        string requestStatus,
        string? needStatus,
        decimal totalAllocated,
        decimal remainingQuantity,
        bool leaveOpen) =>
        new(
            Succeeded: false,
            Status: "OpenUnmatched",
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            RequestId: requestId,
            NeedId: needId,
            RequestStatus: requestStatus,
            NeedStatus: needStatus,
            TotalAllocatedQuantity: totalAllocated,
            RemainingRequestQuantity: remainingQuantity,
            LeaveOpen: leaveOpen,
            SilentConversionApplied: false,
            Legs: []);
}
