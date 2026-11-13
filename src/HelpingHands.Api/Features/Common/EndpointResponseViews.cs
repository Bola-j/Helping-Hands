using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    private static object RecipientView(Recipient recipient) => new { recipient.Id, recipient.Reference, recipient.RecipientType, recipient.DisplayName, recipient.ApprovalStatus, recipient.Active, recipient.CreatedAt, recipient.Version };

    private static object RequestView(AssistanceRequest request, IReadOnlyList<RecordedNeed>? needs = null) => new { request.Id, request.Reference, request.IsMultiNeed, request.Category, request.ServiceSubtype, request.Description, request.RequiredQuantity, request.Unit, request.Status, request.CreatedAt, request.Version, Needs = needs ?? [] };

    private static object ReferralView(Referral referral) => new { referral.Id, referral.Reference, referral.RequestId, referral.RecipientId, referral.AssignedQuantity, referral.ScopeDescription, referral.Status, referral.CreatedAt, referral.Version };
}
