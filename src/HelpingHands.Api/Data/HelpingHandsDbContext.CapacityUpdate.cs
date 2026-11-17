using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureCapacityUpdate(ModelBuilder model) =>
        model.Entity<CapacityUpdate>(builder =>
        {
            builder.ToTable("CapacityUpdate", table =>
                table.HasCheckConstraint("CK_CapacityUpdate_Available_NonNegative", "\"Available\" >= 0"));
            builder.HasKey(x => x.CapacityUpdateId);
            builder.Property(x => x.Available).HasPrecision(18, 3);
            builder.HasIndex(x => new { x.CapacityId, x.UpdatedAt });
            builder.HasOne<DailyCapacity>().WithMany().HasForeignKey(x => x.CapacityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        });
}