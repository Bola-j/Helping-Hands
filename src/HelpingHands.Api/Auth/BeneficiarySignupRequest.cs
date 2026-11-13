namespace HelpingHands.Api.Auth;

public sealed record BeneficiarySignupRequest(
    string? DisplayName,
    string? Email,
    string? Password,
    string? ContactDetails,
    bool IsFamily);
