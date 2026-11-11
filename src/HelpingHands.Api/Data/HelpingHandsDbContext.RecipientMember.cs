using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureRecipientMember(ModelBuilder model) =>
        model.Entity<RecipientMember>(builder =>
        {
            builder.ToTable("RecipientMember");
            builder.HasKey(entity => entity.Id);
            builder.HasIndex(entity => new { entity.RecipientId, entity.UserId }).IsUnique();
            builder.HasIndex(entity => new { entity.UserId, entity.IsActive });
            builder.HasOne<Recipient>().WithMany().HasForeignKey(entity => entity.RecipientId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<AppUser>().WithMany().HasForeignKey(entity => entity.UserId).OnDelete(DeleteBehavior.NoAction);
        });
}
