namespace HelpingHands.Api.Features;

public sealed record SingleNeedRequestInput(Guid BeneficiaryId, string Description, bool AuthorityConfirmed, bool SharingAgreed);
