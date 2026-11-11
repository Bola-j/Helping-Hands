using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureAppRole(ModelBuilder model) =>
        model.Entity<AppRole>(builder =>
        {
            builder.ToTable("Role");
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Name).HasMaxLength(40).IsRequired();
            builder.Property(entity => entity.NormalizedName).HasMaxLength(40).IsRequired();
            builder.Property(entity => entity.ConcurrencyStamp).IsConcurrencyToken();
            builder.HasIndex(entity => entity.NormalizedName).IsUnique();
        });
}
