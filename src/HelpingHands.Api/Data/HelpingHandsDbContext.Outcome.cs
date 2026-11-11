using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureOutcome(ModelBuilder model) =>
        model.Entity<Outcome>(builder =>
        {
            builder.ToTable("Outcome", table =>
            {
                table.HasCheckConstraint("CK_Outcome_Quantity", "\"ReportedQuantity\" > 0");
                table.HasCheckConstraint("CK_Outcome_Status", "\"Status\" IN ('Draft', 'Submitted', 'Clarification', 'Accepted', 'Unresolved')");
            });
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => entity.Reference).IsUnique();
            builder.HasIndex(entity => new { entity.ReferralId, entity.Status });
            builder.Property(entity => entity.Reference).HasMaxLength(32).IsRequired();
            builder.Property(entity => entity.ReportedQuantity).HasPrecision(18, 3);
            builder.Property(entity => entity.Description).HasMaxLength(4000).IsRequired();
            builder.Property(entity => entity.Status).HasMaxLength(30).IsRequired();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.HasOne<Referral>().WithMany().HasForeignKey(entity => entity.ReferralId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.ReportedByUserId).OnDelete(DeleteBehavior.NoAction);
        });
}
