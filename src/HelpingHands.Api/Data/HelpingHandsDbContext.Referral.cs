using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureReferral(ModelBuilder model) =>
        model.Entity<Referral>(builder =>
        {
            ConfigureReferralTable(builder);
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => entity.Reference).IsUnique();
            builder.HasIndex(entity => new { entity.RequestId, entity.Id });
            builder.HasIndex(entity => new { entity.RecipientId, entity.Status });
            builder.Property(entity => entity.Reference).HasMaxLength(32).IsRequired();
            builder.Property(entity => entity.AssignedQuantity).HasPrecision(18, 3);
            builder.Property(entity => entity.ScopeDescription).HasMaxLength(2000).IsRequired();
            builder.Property(entity => entity.Status).HasMaxLength(30).IsRequired();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.HasOne<AssistanceRequest>().WithMany().HasForeignKey(entity => entity.RequestId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<Recipient>().WithMany().HasForeignKey(entity => entity.RecipientId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
        });

    private static void ConfigureReferralTable(EntityTypeBuilder<Referral> builder) =>
        builder.ToTable("Referral", table =>
        {
            table.HasCheckConstraint("CK_Referral_Quantity", "\"AssignedQuantity\" > 0");
            table.HasCheckConstraint("CK_Referral_Status", "\"Status\" IN ('Assigned', 'PartiallyFulfilled', 'Completed', 'Released', 'Declined', 'Unresolved')");
        });
}
