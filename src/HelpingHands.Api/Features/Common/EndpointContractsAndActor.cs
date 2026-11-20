using System.Security.Claims;
using HelpingHands.Api.Data;
using HelpingHands.Api.Features.Needs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace HelpingHands.Api.Features;

public static partial class DemoEndpoints
{
    public sealed record RecipientInput(string DisplayName, string ContactDetails, string? RegistrationInformation);

    public sealed record RecipientDecisionInput(string Decision, string? Reason, Guid Version);

    public sealed record BeneficiaryInput(string DisplayName, string? ContactDetails, bool IsFamily);

    public sealed record RequestInput(Guid BeneficiaryId, string Description, bool AuthorityConfirmed, bool SharingAgreed, IReadOnlyList<NeedSelection?>? Needs);

    public sealed record RequestDecisionInput(string Decision, string? Reason, string? Category, string? ServiceSubtype, decimal? RequiredQuantity, string? Unit, Guid Version);

    public sealed record ReferralInput(
        Guid RecipientId,
        decimal AssignedQuantity,
        string ScopeDescription,
        Guid Version,
        Guid? ServiceVersionId = null,
        Guid? StandardVersionId = null);

    public sealed record OutcomeInput(decimal ReportedQuantity, string Description);

    public sealed record VersionInput(Guid Version);

    public sealed record UpdateRequestInput(string Description, Guid Version);

    public sealed record UpdateOutcomeInput(decimal ReportedQuantity, string Description, Guid Version);

    public sealed record ReviewInput(string Decision, string? Reason, Guid Version);

    private static Guid Actor(ClaimsPrincipal user) => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static bool Coordinator(ClaimsPrincipal user) => user.IsInRole("Coordinator") || Admin(user);

    private static bool Admin(ClaimsPrincipal user) => user.IsInRole("Administrator");
}