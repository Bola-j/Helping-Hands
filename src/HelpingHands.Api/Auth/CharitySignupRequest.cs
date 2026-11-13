namespace HelpingHands.Api.Auth;

public sealed record CharitySignupRequest(
    string? RepresentativeDisplayName,
    string? Email,
    string? Password,
    string? OrganizationName,
    string? ContactDetails,
    string? RegistrationInformation);
