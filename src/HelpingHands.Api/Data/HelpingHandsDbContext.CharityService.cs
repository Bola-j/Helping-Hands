using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureCharityService(ModelBuilder model) =>
        model.Entity<CharityService>(builder =>
        {
            builder.ToTable("CharityService");
            builder.HasKey(x => x.CharityServiceId);
            builder.HasIndex(x => new { x.CharityId, x.ServiceVersionId, x.StandardVersionId }).IsUnique();
            builder.HasOne<Charity>().WithMany().HasForeignKey(x => x.CharityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<ServiceVersion>().WithMany().HasForeignKey(x => x.ServiceVersionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<StandardVersion>().WithMany().HasForeignKey(x => x.StandardVersionId).OnDelete(DeleteBehavior.Restrict);
        });
}