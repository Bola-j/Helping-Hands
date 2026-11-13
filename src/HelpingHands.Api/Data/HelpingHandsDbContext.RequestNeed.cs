using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureRequestNeed(ModelBuilder model) => model.Entity<RequestNeed>(builder =>
    {
        builder.ToTable("RequestNeed", table => table.HasCheckConstraint("CK_RequestNeed_Quantity", "\"Quantity\" > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => new { x.RequestId, x.StandardVersionId }).IsUnique();
        builder.HasOne<AssistanceRequest>().WithMany().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StandardVersion>().WithMany().HasForeignKey(x => x.StandardVersionId).OnDelete(DeleteBehavior.Restrict);
    });
}
