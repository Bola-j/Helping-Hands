using System.Data;
using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;

namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static async Task<IResult> CreateRequest(RequestInput input, ClaimsPrincipal user, HelpingHandsDbContext database, NeedVersionService versions, CancellationToken ct)
    {
        if (!Beneficiary(user)) return Results.Forbid();
        if (!input.AuthorityConfirmed || !input.SharingAgreed || Blank(input.Description, 4000) || input.Needs is null || input.Needs.Count is 0 or > 50 || input.Needs.Any(x => x is null))
            return Bad("Description, authority, sharing confirmations and 1 to 50 needs are required.");
        var userId = Actor(user);
        if (!await database.Beneficiaries.AnyAsync(x => x.Id == input.BeneficiaryId && x.LinkedUserId == userId, ct)) return Results.NotFound();
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var request = new AssistanceRequest { BeneficiaryId = input.BeneficiaryId, SubmittedByUserId = userId, Description = input.Description.Trim(), AuthorityConfirmed = true, SharingAgreed = true, IsMultiNeed = true, Reference = await database.NextReferenceAsync("REQ", ct) };
        database.Requests.Add(request);
        database.DecisionHistory.Add(new DecisionHistory { RequestId = request.Id, ActorUserId = userId, Action = "Submitted" });
        await database.SaveChangesAsync(ct);
        var result = await versions.AddNeedsAsync(request.Id, input.Needs.Select(x => x!).ToList(), ct);
        if (result != NeedCreationResult.Created) return result == NeedCreationResult.StaleSelection ? Conflict("A selected standard is no longer current.") : Bad("Invalid needs.");
        var recorded = await versions.GetRecordedNeedsAsync(request.Id, ct);
        await transaction.CommitAsync(ct);
        return Results.Created($"/api/v1/requests/{request.Id}", RequestView(request, recorded));
    }

    private static async Task<IResult> ListRequests(string? reference, string? cursor, int? limit, HttpResponse response, ClaimsPrincipal user, HelpingHandsDbContext database, NeedVersionService versions, CancellationToken ct)
    {
        if (limit is < 1 or > 100 || !TryRequestCursor(cursor, out var after)) return Bad("Invalid cursor or page size.");
        var pageSize = limit ?? 25;
        var query = VisibleRequests(database, user);
        if (!string.IsNullOrWhiteSpace(reference)) query = query.Where(request => request.Reference == reference);
        if (after is not null) query = query.Where(request => request.Id.CompareTo(after.Value) > 0);
        var requests = await query.OrderBy(request => request.Id).Take(pageSize + 1).ToListAsync(ct);
        if (requests.Count > pageSize)
        {
            requests.RemoveAt(pageSize);
            response.Headers["X-Next-Cursor"] = WebEncoders.Base64UrlEncode(requests[^1].Id.ToByteArray());
        }
        var needs = await versions.GetRecordedNeedsForRequestsAsync(requests.Where(x => x.IsMultiNeed).Select(x => x.Id).ToArray(), ct);
        return Results.Ok(requests.Select(request => RequestView(request, needs.GetValueOrDefault(request.Id))));
    }
}
