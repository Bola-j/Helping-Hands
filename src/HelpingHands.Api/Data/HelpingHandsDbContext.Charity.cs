using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureCharity(ModelBuilder model) =>
        model.Entity<Charity>(builder =>
        {
            builder.ToTable("Charity");
            builder.HasKey(entity => entity.RecipientId);
            builder.Property(entity => entity.RegistrationInformation).HasMaxLength(1000);
            builder.HasOne<Recipient>().WithOne().HasForeignKey<Charity>(entity => entity.RecipientId).OnDelete(DeleteBehavior.NoAction);
        });
}
