using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureBeneficiary(ModelBuilder model) =>
        model.Entity<Beneficiary>(builder =>
        {
            builder.ToTable("Beneficiary");
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => entity.Reference).IsUnique();
            builder.HasIndex(entity => entity.LinkedUserId).IsUnique().HasFilter("\"LinkedUserId\" IS NOT NULL");
            builder.HasIndex(entity => entity.CreatedByUserId);
            builder.Property(entity => entity.Reference).HasMaxLength(32).IsRequired();
            builder.Property(entity => entity.DisplayName).HasMaxLength(200).IsRequired();
            builder.Property(entity => entity.ContactDetails).HasMaxLength(500);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.LinkedUserId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
        });
}
