using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpingHands.Api.Migrations
{
    /// <inheritdoc />
    public partial class DailyReminderPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyReminderIntent",
                columns: table => new
                {
                    IntentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CairoDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduledForUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReminderIntent", x => x.IntentId);
                    table.CheckConstraint("CK_DailyReminderIntent_AttemptCount", "\"AttemptCount\" >= 0");
                    table.CheckConstraint("CK_DailyReminderIntent_DeliveredAt", "(\"Status\" = 'Delivered' AND \"DeliveredAt\" IS NOT NULL) OR (\"Status\" <> 'Delivered' AND \"DeliveredAt\" IS NULL)");
                    table.CheckConstraint("CK_DailyReminderIntent_Status", "\"Status\" IN ('Pending', 'Delivered', 'Failed')");
                    table.ForeignKey(
                        name: "FK_DailyReminderIntent_Charity_CharityId",
                        column: x => x.CharityId,
                        principalTable: "Charity",
                        principalColumn: "RecipientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReminderDeliveryAttempt",
                columns: table => new
                {
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecipientEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderDeliveryAttempt", x => x.AttemptId);
                    table.CheckConstraint("CK_ReminderDeliveryAttempt_Number", "\"AttemptNumber\" > 0");
                    table.CheckConstraint("CK_ReminderDeliveryAttempt_Status", "\"Status\" IN ('InProgress', 'Succeeded', 'Failed')");
                    table.ForeignKey(
                        name: "FK_ReminderDeliveryAttempt_DailyReminderIntent_IntentId",
                        column: x => x.IntentId,
                        principalTable: "DailyReminderIntent",
                        principalColumn: "IntentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyReminderIntent_CairoDate_Status",
                table: "DailyReminderIntent",
                columns: new[] { "CairoDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyReminderIntent_CharityId_CairoDate",
                table: "DailyReminderIntent",
                columns: new[] { "CharityId", "CairoDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReminderDeliveryAttempt_IntentId_AttemptNumber",
                table: "ReminderDeliveryAttempt",
                columns: new[] { "IntentId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReminderDeliveryAttempt_IntentId_StartedAt",
                table: "ReminderDeliveryAttempt",
                columns: new[] { "IntentId", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReminderDeliveryAttempt");

            migrationBuilder.DropTable(
                name: "DailyReminderIntent");
        }
    }
}
