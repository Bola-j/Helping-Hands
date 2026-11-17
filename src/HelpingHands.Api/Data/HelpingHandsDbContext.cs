using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext(DbContextOptions<HelpingHandsDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<AppRole> Roles => Set<AppRole>();
    public DbSet<AppUserRole> UserRoles => Set<AppUserRole>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<Recipient> Recipients => Set<Recipient>();
    public DbSet<Charity> Charities => Set<Charity>();
    public DbSet<RecipientMember> RecipientMembers => Set<RecipientMember>();
    public DbSet<RecipientDecision> RecipientDecisions => Set<RecipientDecision>();
    public DbSet<NeedReviewHistory> NeedReviewHistory => Set<NeedReviewHistory>();
    public DbSet<RequestNeed> RequestNeeds => Set<RequestNeed>();
    public DbSet<AssistanceRequest> Requests => Set<AssistanceRequest>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<Outcome> Outcomes => Set<Outcome>();
    public DbSet<OutcomeReview> OutcomeReviews => Set<OutcomeReview>();
    public DbSet<ApplicationAttachment> ApplicationAttachments => Set<ApplicationAttachment>();
    public DbSet<EvidenceAttachment> EvidenceAttachments => Set<EvidenceAttachment>();
    public DbSet<DecisionHistory> DecisionHistory => Set<DecisionHistory>();

    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceVersion> ServiceVersions => Set<ServiceVersion>();
    public DbSet<UnitStandard> UnitStandards => Set<UnitStandard>();
    public DbSet<StandardVersion> StandardVersions => Set<StandardVersion>();
    public DbSet<StandardComponent> StandardComponents => Set<StandardComponent>();

    public DbSet<CharityService> CharityServices => Set<CharityService>();
    public DbSet<DailyCapacity> DailyCapacities => Set<DailyCapacity>();
    public DbSet<CapacityUpdate> CapacityUpdates => Set<CapacityUpdate>();

    // PostgreSQL sequences are nontransactional; gaps after rollback are expected.
    public async Task<string> NextReferenceAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var sequence = prefix switch
        {
            "BEN" => "ben_reference_seq",
            "REC" => "rec_reference_seq",
            "REQ" => "req_reference_seq",
            "REF" => "ref_reference_seq",
            "OUT" => "out_reference_seq",
            _ => throw new ArgumentOutOfRangeException(nameof(prefix))
        };
        // Bind the allowlisted sequence name as a PostgreSQL regclass parameter.
        var value = await Database.SqlQuery<long>($"SELECT nextval({sequence}::regclass) AS \"Value\"")
            .SingleAsync(cancellationToken);
        return $"{prefix}-{value:D8}";
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        foreach (var sequence in new[] { "ben_reference_seq", "rec_reference_seq", "req_reference_seq", "ref_reference_seq", "out_reference_seq" })
            model.HasSequence<long>(sequence);
        ConfigureServiceCategory(model);
        ConfigureServiceVersion(model);
        ConfigureUnitStandard(model);
        ConfigureStandardVersion(model);
        ConfigureStandardComponent(model);
        ConfigureAppUser(model);
        ConfigureAppRole(model);
        ConfigureAppUserRole(model);
        ConfigureBeneficiary(model);
        ConfigureRecipient(model);
        ConfigureCharity(model);
        ConfigureRecipientMember(model);
        ConfigureRecipientDecision(model);
        ConfigureAssistanceRequest(model);
        ConfigureRequestNeed(model);
        ConfigureNeedReviewHistory(model);
        ConfigureReferral(model);
        ConfigureOutcome(model);
        ConfigureOutcomeReview(model);
        ConfigureApplicationAttachment(model);
        ConfigureEvidenceAttachment(model);
        ConfigureDecisionHistory(model);
        ConfigureCapacitySchema(model);
    }

    private static void ConfigureCapacitySchema(ModelBuilder model)
    {
        ConfigureCharityService(model);
        ConfigureDailyCapacity(model);
        ConfigureCapacityUpdate(model);
    }
}