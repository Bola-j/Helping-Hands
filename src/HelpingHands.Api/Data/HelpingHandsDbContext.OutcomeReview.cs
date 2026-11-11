using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureOutcomeReview(ModelBuilder model) =>
        model.Entity<OutcomeReview>(builder =>
        {
            builder.ToTable("OutcomeReview", table => table.HasCheckConstraint("CK_OutcomeReview_Decision", "\"Decision\" IN ('Accepted', 'Clarification', 'Unresolved')"));
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => new { entity.OutcomeId, entity.ReviewedAt });
            builder.Property(entity => entity.Decision).HasMaxLength(30).IsRequired();
            builder.Property(entity => entity.Reason).HasMaxLength(2000);
            builder.HasOne<Outcome>().WithMany().HasForeignKey(entity => entity.OutcomeId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.ActorUserId).OnDelete(DeleteBehavior.NoAction);
        });
}
