using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureAppUserRole(ModelBuilder model) =>
        model.Entity<AppUserRole>(builder =>
        {
            builder.ToTable("UserRole");
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => new { entity.UserId, entity.RoleId }).IsUnique();
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.UserId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppRole>().WithMany().HasForeignKey(entity => entity.RoleId).OnDelete(DeleteBehavior.NoAction);
        });
}
