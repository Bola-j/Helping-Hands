using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureAppUser(ModelBuilder model) =>
        model.Entity<AppUser>(builder =>
        {
            builder.ToTable("UserAccount", table => table.HasCheckConstraint("CK_UserAccount_Status", "\"AccountStatus\" IN ('Pending', 'Active', 'Disabled')"));
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.UserName).HasMaxLength(256).IsRequired();
            builder.Property(entity => entity.NormalizedUserName).HasMaxLength(256).IsRequired();
            builder.Property(entity => entity.Email).HasMaxLength(320);
            builder.Property(entity => entity.NormalizedEmail).HasMaxLength(320);
            builder.Property(entity => entity.DisplayName).HasMaxLength(200).IsRequired();
            builder.Property(entity => entity.AccountStatus).HasMaxLength(20).IsRequired();
            builder.Property(entity => entity.ConcurrencyStamp).IsConcurrencyToken();
            builder.HasIndex(entity => entity.NormalizedUserName).IsUnique();
            builder.HasIndex(entity => entity.NormalizedEmail).IsUnique().HasFilter("\"NormalizedEmail\" IS NOT NULL");
        });
}
