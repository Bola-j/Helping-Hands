using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureEvidenceAttachment(ModelBuilder model) =>
        model.Entity<EvidenceAttachment>(builder =>
        {
            builder.ToTable("EvidenceAttachment", table => table.HasCheckConstraint("CK_EvidenceAttachment_Size", "\"SizeBytes\" > 0"));
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => entity.StorageKey).IsUnique();
            builder.HasIndex(entity => entity.OutcomeId);
            builder.Property(entity => entity.StorageKey).HasMaxLength(500).IsRequired();
            builder.Property(entity => entity.OriginalName).HasMaxLength(255).IsRequired();
            builder.Property(entity => entity.MediaType).HasMaxLength(150).IsRequired();
            builder.HasOne<Outcome>().WithMany().HasForeignKey(entity => entity.OutcomeId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.UploadedByUserId).OnDelete(DeleteBehavior.NoAction);
        });
}
