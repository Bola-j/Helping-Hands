using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Data;

public sealed partial class HelpingHandsDbContext
{
    private static void ConfigureReminderSchema(ModelBuilder model)
    {
        ConfigureDailyReminderIntent(model);
        ConfigureReminderDeliveryAttempt(model);
    }

    private static void ConfigureDailyReminderIntent(ModelBuilder model) =>
        model.Entity<DailyReminderIntent>(builder =>
        {
            builder.ToTable("DailyReminderIntent", table =>
            {
                table.HasCheckConstraint("CK_DailyReminderIntent_Status", "\"Status\" IN ('Pending', 'Delivered', 'Failed')");
                table.HasCheckConstraint("CK_DailyReminderIntent_AttemptCount", "\"AttemptCount\" >= 0");
                table.HasCheckConstraint("CK_DailyReminderIntent_DeliveredAt", "(\"Status\" = 'Delivered' AND \"DeliveredAt\" IS NOT NULL) OR (\"Status\" <> 'Delivered' AND \"DeliveredAt\" IS NULL)");
            });
            builder.HasKey(x => x.IntentId);
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Version).IsConcurrencyToken();
            builder.HasIndex(x => new { x.CharityId, x.CairoDate }).IsUnique();
            builder.HasIndex(x => new { x.CairoDate, x.Status });
            builder.HasOne<Charity>().WithMany().HasForeignKey(x => x.CharityId).OnDelete(DeleteBehavior.Restrict);
        });

    private static void ConfigureReminderDeliveryAttempt(ModelBuilder model) =>
        model.Entity<ReminderDeliveryAttempt>(builder =>
        {
            builder.ToTable("ReminderDeliveryAttempt", table =>
            {
                table.HasCheckConstraint("CK_ReminderDeliveryAttempt_Number", "\"AttemptNumber\" > 0");
                table.HasCheckConstraint("CK_ReminderDeliveryAttempt_Status", "\"Status\" IN ('InProgress', 'Succeeded', 'Failed')");
            });
            builder.HasKey(x => x.AttemptId);
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.RecipientEmail).HasMaxLength(320);
            builder.Property(x => x.FailureReason).HasMaxLength(2000);
            builder.HasIndex(x => new { x.IntentId, x.AttemptNumber }).IsUnique();
            builder.HasIndex(x => new { x.IntentId, x.StartedAt });
            builder.HasOne<DailyReminderIntent>().WithMany().HasForeignKey(x => x.IntentId).OnDelete(DeleteBehavior.Restrict);
        });
}