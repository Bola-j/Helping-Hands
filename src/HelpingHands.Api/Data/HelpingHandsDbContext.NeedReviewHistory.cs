using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureNeedReviewHistory(ModelBuilder model) => model.Entity<NeedReviewHistory>(builder =>
    {
        builder.ToTable("NeedReviewHistory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.Property(x => x.Category).HasMaxLength(30);
        builder.Property(x => x.ServiceSubtype).HasMaxLength(100);
        builder.HasIndex(x => new { x.NeedId, x.OccurredAt });
        builder.HasOne<RequestNeed>().WithMany().HasForeignKey(x => x.NeedId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    });
}
