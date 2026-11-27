namespace HelpingHands.Api.Features.Progress;

public sealed record RequestProgressView(Guid Id, string Reference, string? Status, string? Description,
    IReadOnlyList<NeedProgressView> Needs, IReadOnlyList<ProgressHistoryEvent> History);

public sealed record NeedProgressView(Guid? NeedId, string? Service, string? Standard, decimal? RequiredQuantity,
    string? Status, decimal AssignedQuantity, decimal AcceptedQuantity, decimal? RemainingQuantity,
    IReadOnlyList<AssignmentProgressView> Assignments);

public sealed record AssignmentProgressView(Guid Id, string Reference, string ScopeDescription,
    decimal AssignedQuantity, decimal AcceptedQuantity, string Status, IReadOnlyList<OutcomeProgressView> Outcomes);

public sealed record OutcomeProgressView(Guid Id, string Reference, decimal ReportedQuantity, string Description,
    string Status, DateTimeOffset? SubmittedAt, Guid Version, IReadOnlyList<EvidenceAttachmentProgressView> Evidence);

public sealed record EvidenceAttachmentProgressView(Guid Id, string OriginalName, string MediaType,
    long SizeBytes, DateTimeOffset UploadedAt);

public sealed record ProgressHistoryEvent(string EntityType, Guid EntityId, string Action,
    DateTimeOffset OccurredAt, Guid? ActorUserId, string? Reason);
