using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureApplicationAttachment(ModelBuilder model) =>
        model.Entity<ApplicationAttachment>(builder =>
        {
            builder.ToTable("ApplicationAttachment", table => table.HasCheckConstraint("CK_ApplicationAttachment_Size", "\"SizeBytes\" > 0"));
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => entity.StorageKey).IsUnique();
            builder.Property(entity => entity.StorageKey).HasMaxLength(500).IsRequired();
            builder.Property(entity => entity.OriginalName).HasMaxLength(255).IsRequired();
            builder.Property(entity => entity.MediaType).HasMaxLength(150).IsRequired();
            builder.HasOne<Recipient>().WithMany().HasForeignKey(entity => entity.RecipientId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.UploadedByUserId).OnDelete(DeleteBehavior.NoAction);
        });
}
