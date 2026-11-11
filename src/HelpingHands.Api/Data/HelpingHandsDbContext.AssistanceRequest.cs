using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureAssistanceRequest(ModelBuilder model) =>
        model.Entity<AssistanceRequest>(builder =>
        {
            ConfigureRequestTable(builder);
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => entity.Reference).IsUnique();
            builder.HasIndex(entity => new { entity.BeneficiaryId, entity.CreatedAt });
            builder.HasIndex(entity => new { entity.SubmittedByUserId, entity.CreatedAt });
            builder.HasIndex(entity => new { entity.Status, entity.CreatedAt });
            builder.Property(entity => entity.Reference).HasMaxLength(32).IsRequired();
            builder.Property(entity => entity.Category).HasMaxLength(30);
            builder.Property(entity => entity.ServiceSubtype).HasMaxLength(100);
            builder.Property(entity => entity.Description).HasMaxLength(4000).IsRequired();
            builder.Property(entity => entity.RequiredQuantity).HasPrecision(18, 3);
            builder.Property(entity => entity.Unit).HasMaxLength(50);
            builder.Property(entity => entity.Status).HasMaxLength(30).IsRequired();
            builder.Property(entity => entity.Version).IsConcurrencyToken();
            builder.HasOne<Beneficiary>().WithMany().HasForeignKey(entity => entity.BeneficiaryId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.SubmittedByUserId).OnDelete(DeleteBehavior.NoAction);
        });

    private static void ConfigureRequestTable(EntityTypeBuilder<AssistanceRequest> builder) =>
        builder.ToTable("AssistanceRequest", table =>
        {
            table.HasCheckConstraint("CK_Request_Category", "\"Category\" IS NULL OR \"Category\" IN ('Goods', 'Money', 'OrganizationalReferral')");
            table.HasCheckConstraint("CK_Request_Status", "\"Status\" IN ('Submitted', 'Clarification', 'Accepted', 'Rejected', 'Open', 'Completed', 'ClosedUnresolved', 'Split')");
            table.HasCheckConstraint("CK_Request_Quantity", "\"RequiredQuantity\" IS NULL OR \"RequiredQuantity\" > 0");
            table.HasCheckConstraint("CK_Request_Confirmed", "\"AuthorityConfirmed\" AND \"SharingAgreed\"");
        });
}
