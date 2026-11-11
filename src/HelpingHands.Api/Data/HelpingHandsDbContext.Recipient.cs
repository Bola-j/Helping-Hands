using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureRecipient(ModelBuilder model) =>
        model.Entity<Recipient>(builder =>
        {
            builder.ToTable("ReferralRecipient", table =>
            {
                table.HasCheckConstraint("CK_Recipient_Type", "\"RecipientType\" IN ('Charity', 'Contributor')");
                table.HasCheckConstraint("CK_Recipient_Approval", "\"ApprovalStatus\" IN ('Pending', 'Approved', 'Rejected')");
                table.HasCheckConstraint("CK_Recipient_Active", "NOT \"Active\" OR \"ApprovalStatus\" = 'Approved'");
            });
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => entity.Reference).IsUnique();
            builder.HasIndex(entity => new { entity.ApprovalStatus, entity.Active });
            builder.Property(entity => entity.Reference).HasMaxLength(32).IsRequired();
            builder.Property(entity => entity.RecipientType).HasMaxLength(20).IsRequired();
            builder.Property(entity => entity.DisplayName).HasMaxLength(200).IsRequired();
            builder.Property(entity => entity.ContactDetails).HasMaxLength(500).IsRequired();
            builder.Property(entity => entity.ApprovalStatus).HasMaxLength(20).IsRequired();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
        });
}
