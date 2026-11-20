using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Reminders;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features.Referrals;

public sealed record ExactVersionResolutionCandidate(
    Guid RecipientId,
    string CharityName,
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
    decimal AvailableCapacity,
    DateTimeOffset CapacityUpdatedAt);

public sealed record ExactVersionResolutionResult(
    Guid? RequestId,
    Guid? NeedId,
    string RequestStatus,
    string NeedStatus,
    string ResolutionStatus,
    bool LeaveOpen,
    bool SilentConversionApplied,
    DateOnly CairoDate,
    Guid? ServiceCategoryId,
    Guid? ServiceVersionId,
    Guid? StandardVersionId,
    IReadOnlyList<ExactVersionResolutionCandidate> Candidates,
    int PreservedHistoryCount);

public sealed class ExactVersionRequestResolutionService(
    HelpingHandsDbContext database,
    TimeProvider timeProvider)
{
    public async Task<ExactVersionResolutionResult> ResolveExactVersionAsync(
        Guid? requestId,
        Guid? needId,
        Guid? serviceCategoryId,
        Guid? serviceVersionId,
        Guid? standardVersionId,
        decimal? requiredQuantity,
        CancellationToken cancellationToken = default)
    {
        var cairoDate = DailyReminderStateService.GetCairoDate(timeProvider.GetUtcNow());

        AssistanceRequest? request = null;
        if (requestId.HasValue)
        {
            request = await database.Requests.AsNoTracking()
                .SingleOrDefaultAsync(r => r.Id == requestId.Value, cancellationToken);
        }

        RequestNeed? need = null;
        if (needId.HasValue)
        {
            need = await database.RequestNeeds.AsNoTracking()
                .SingleOrDefaultAsync(n => n.Id == needId.Value, cancellationToken);
        }

        var effectiveStandardVersionId = standardVersionId ?? need?.StandardVersionId;
        var effectiveQuantity = requiredQuantity ?? need?.Quantity ?? request?.RequiredQuantity;

        var historyCount = 0;
        if (request is not null)
        {
            historyCount += await database.DecisionHistory.AsNoTracking()
                .CountAsync(h => h.RequestId == request.Id, cancellationToken);
        }

        var currentRequestStatus = string.IsNullOrWhiteSpace(request?.Status) ? "Open" : request.Status;
        var currentNeedStatus = string.IsNullOrWhiteSpace(need?.Status) ? "Open" : need.Status;

        if (!serviceCategoryId.HasValue || !serviceVersionId.HasValue || !effectiveStandardVersionId.HasValue)
        {
            return new ExactVersionResolutionResult(
                RequestId: request?.Id ?? requestId,
                NeedId: need?.Id ?? needId,
                RequestStatus: currentRequestStatus,
                NeedStatus: currentNeedStatus,
                ResolutionStatus: "OpenUnmatched",
                LeaveOpen: true,
                SilentConversionApplied: false,
                CairoDate: cairoDate,
                ServiceCategoryId: serviceCategoryId,
                ServiceVersionId: serviceVersionId,
                StandardVersionId: effectiveStandardVersionId,
                Candidates: [],
                PreservedHistoryCount: historyCount);
        }

        var rows = await (
            from cs in database.CharityServices.AsNoTracking()
            join r in database.Recipients.AsNoTracking() on cs.CharityId equals r.Id
            join c in database.Charities.AsNoTracking() on cs.CharityId equals c.RecipientId
            join sv in database.ServiceVersions.AsNoTracking() on cs.ServiceVersionId equals sv.Id
            join sc in database.ServiceCategories.AsNoTracking() on sv.CategoryId equals sc.Id
            join stv in database.StandardVersions.AsNoTracking() on cs.StandardVersionId equals stv.Id
            join us in database.UnitStandards.AsNoTracking() on stv.UnitStandardId equals us.Id
            join dc in database.DailyCapacities.AsNoTracking()
                on cs.CharityServiceId equals dc.CharityServiceId into capacities
            from todayCap in capacities.Where(dc => dc.CairoDate == cairoDate).DefaultIfEmpty()
            where cs.Enabled
                && r.RecipientType == "Charity"
                && r.ApprovalStatus == "Approved"
                && r.Active
                && sv.CategoryId == serviceCategoryId.Value
                && cs.ServiceVersionId == serviceVersionId.Value
                && cs.StandardVersionId == effectiveStandardVersionId.Value
            select new
            {
                RecipientId = r.Id,
                CharityName = r.DisplayName,
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

        var minFloor = effectiveQuantity.HasValue && effectiveQuantity.Value > 0m
            ? effectiveQuantity.Value
            : 0.0000000001m;

        var candidates = rows
            .Where(x => x.HasFreshCapacity && x.Available >= minFloor)
            .OrderByDescending(x => x.Available)
            .ThenBy(x => x.CharityName)
            .Select(x => new ExactVersionResolutionCandidate(
                RecipientId: x.RecipientId,
                CharityName: x.CharityName,
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
                AvailableCapacity: x.Available,
                CapacityUpdatedAt: x.UpdatedAt))
            .ToList();

        var hasMatch = candidates.Count > 0;

        return new ExactVersionResolutionResult(
            RequestId: request?.Id ?? requestId,
            NeedId: need?.Id ?? needId,
            RequestStatus: currentRequestStatus,
            NeedStatus: currentNeedStatus,
            ResolutionStatus: hasMatch ? "Matched" : "OpenUnmatched",
            LeaveOpen: !hasMatch,
            SilentConversionApplied: false,
            CairoDate: cairoDate,
            ServiceCategoryId: serviceCategoryId.Value,
            ServiceVersionId: serviceVersionId.Value,
            StandardVersionId: effectiveStandardVersionId.Value,
            Candidates: candidates,
            PreservedHistoryCount: historyCount);
    }
}