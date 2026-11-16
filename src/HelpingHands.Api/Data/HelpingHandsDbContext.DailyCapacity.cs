using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureDailyCapacity(ModelBuilder model) =>
        model.Entity<DailyCapacity>(builder =>
        {
            builder.ToTable("DailyCapacity", table =>
                table.HasCheckConstraint("CK_DailyCapacity_Available_NonNegative", "\"Available\" >= 0"));
            builder.HasKey(x => x.CapacityId);
            builder.Property(x => x.Available).HasPrecision(18, 3);
            builder.HasIndex(x => new { x.CharityServiceId, x.CairoDate }).IsUnique();
            builder.HasOne<CharityService>().WithMany().HasForeignKey(x => x.CharityServiceId).OnDelete(DeleteBehavior.Restrict);
        });
}