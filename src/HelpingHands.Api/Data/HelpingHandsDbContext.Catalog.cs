using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureServiceCategory(ModelBuilder model) => model.Entity<ServiceCategory>(builder =>
    {
        builder.ToTable("ServiceCategory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    });

    private static void ConfigureServiceVersion(ModelBuilder model) => model.Entity<ServiceVersion>(builder =>
    {
        builder.ToTable("ServiceVersion", table => table.HasCheckConstraint("CK_ServiceVersion_Number", "\"VersionNumber\" > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.CategoryId, x.VersionNumber }).IsUnique();
        builder.HasOne<ServiceCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    });

    private static void ConfigureUnitStandard(ModelBuilder model) => model.Entity<UnitStandard>(builder =>
    {
        builder.ToTable("UnitStandard");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasOne<ServiceVersion>().WithMany().HasForeignKey(x => x.ServiceVersionId).OnDelete(DeleteBehavior.Restrict);
    });
}
