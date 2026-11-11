using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureDecisionHistory(ModelBuilder model) =>
        model.Entity<DecisionHistory>(builder =>
        {
            builder.ToTable("DecisionHistory", table => table.HasCheckConstraint("CK_DecisionHistory_Subject", "(\"RequestId\" IS NOT NULL) <> (\"ReferralId\" IS NOT NULL)"));
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => new { entity.RequestId, entity.OccurredAt });
            builder.HasIndex(entity => new { entity.ReferralId, entity.OccurredAt });
            builder.Property(entity => entity.Action).HasMaxLength(50).IsRequired();
            builder.Property(entity => entity.Reason).HasMaxLength(2000);
            builder.HasOne<AssistanceRequest>().WithMany().HasForeignKey(entity => entity.RequestId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<Referral>().WithMany().HasForeignKey(entity => entity.ReferralId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.ActorUserId).OnDelete(DeleteBehavior.NoAction);
        });
}
