using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureRecipientDecision(ModelBuilder model) =>
        model.Entity<RecipientDecision>(builder =>
        {
            builder.ToTable("RecipientDecision", table => table.HasCheckConstraint("CK_RecipientDecision_Decision", "\"Decision\" IN ('Pending', 'Approved', 'Rejected')"));
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => new { entity.RecipientId, entity.DecidedAt });
            builder.Property(entity => entity.Decision).HasMaxLength(20).IsRequired();
            builder.Property(entity => entity.Reason).HasMaxLength(2000);
            builder.HasOne<Recipient>().WithMany().HasForeignKey(entity => entity.RecipientId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.ActorUserId).OnDelete(DeleteBehavior.NoAction);
        });
}
