using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureStandardVersion(ModelBuilder model) => model.Entity<StandardVersion>(builder =>
    {
        builder.ToTable("StandardVersion", table => table.HasCheckConstraint("CK_StandardVersion_Number", "\"VersionNumber\" > 0"));
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UnitStandardId, x.VersionNumber }).IsUnique();
        builder.HasOne<UnitStandard>().WithMany().HasForeignKey(x => x.UnitStandardId).OnDelete(DeleteBehavior.Restrict);
    });

    private static void ConfigureStandardComponent(ModelBuilder model) => model.Entity<StandardComponent>(builder =>
    {
        builder.ToTable("StandardComponent", table => table.HasCheckConstraint("CK_StandardComponent_Quantity", "\"Quantity\" > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.HasOne<StandardVersion>().WithMany().HasForeignKey(x => x.StandardVersionId).OnDelete(DeleteBehavior.Restrict);
    });
}
