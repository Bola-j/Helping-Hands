using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using HelpingHands.Api.Features.Progress;
using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IReadOnlyList<NeedProgressView>> NeedProgressAsync(HelpingHandsDbContext database,
        AssistanceRequest request, IReadOnlyList<Referral> referrals, NeedVersionService versions, CancellationToken cancellationToken)
    {
        var referralIds = referrals.Select(item => item.Id).ToArray();
        var outcomes = await database.Outcomes.AsNoTracking().Where(item => referralIds.Contains(item.ReferralId)).ToListAsync(cancellationToken);
        var accepted = outcomes.Where(item => item.Status == "Accepted").GroupBy(item => item.ReferralId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.ReportedQuantity));
        var outcomesByReferral = outcomes.GroupBy(item => item.ReferralId).ToDictionary(group => group.Key,
            group => (IReadOnlyList<OutcomeProgressView>)group.Select(item => new OutcomeProgressView(item.Id, item.Reference,
                item.ReportedQuantity, item.Status, item.SubmittedAt)).ToList());
        var needs = request.IsMultiNeed ? await versions.GetRecordedNeedsAsync(request.Id, cancellationToken) : [];
        var view = needs.Select(item => BuildNeedProgressView(item.Id, item.ServiceName, item.StandardName, item.Quantity,
            item.Status, referrals.Where(referral => referral.NeedId == item.Id), accepted, outcomesByReferral)).ToList();
        if (!request.IsMultiNeed)
            view.Add(BuildNeedProgressView(null, request.Category, request.Unit, request.RequiredQuantity ?? 0m,
                request.Status, referrals, accepted, outcomesByReferral));
        return view;
    }

    private static NeedProgressView BuildNeedProgressView(Guid? needId, string? service, string? standard, decimal quantity,
        string status, IEnumerable<Referral> referralRows, IReadOnlyDictionary<Guid, decimal> accepted,
        IReadOnlyDictionary<Guid, IReadOnlyList<OutcomeProgressView>> outcomes)
    {
        var assignments = referralRows.Select(item => new AssignmentProgressView(item.Id, item.Reference, item.ScopeDescription,
            item.AssignedQuantity, accepted.GetValueOrDefault(item.Id), item.Status,
            outcomes.GetValueOrDefault(item.Id) ?? [])).ToList();
        var assigned = assignments.Sum(item => item.AssignedQuantity);
        var fulfilled = assignments.Sum(item => item.AcceptedQuantity);
        return new NeedProgressView(needId, service, standard, quantity, status, assigned, fulfilled,
            Math.Max(0m, quantity - fulfilled), assignments);
    }
}
