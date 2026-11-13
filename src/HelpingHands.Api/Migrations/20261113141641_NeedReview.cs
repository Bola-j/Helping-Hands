using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpingHands.Api.Migrations
{
    /// <inheritdoc />
    public partial class NeedReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Request_Status",
                table: "AssistanceRequest");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "RequestNeed",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceSubtype",
                table: "RequestNeed",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NeedReviewHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NeedId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ServiceSubtype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeedReviewHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NeedReviewHistory_RequestNeed_NeedId",
                        column: x => x.NeedId,
                        principalTable: "RequestNeed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NeedReviewHistory_UserAccount_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "UserAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RequestNeed_Category",
                table: "RequestNeed",
                sql: "\"Category\" IS NULL OR \"Category\" IN ('Goods', 'Money', 'OrganizationalReferral')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Request_Status",
                table: "AssistanceRequest",
                sql: "\"Status\" IN ('Submitted', 'Clarification', 'Accepted', 'Rejected', 'Open', 'Completed', 'ClosedUnresolved', 'Split', 'InReview', 'FullyAccepted', 'PartiallyAccepted', 'FullyRejected')");

            migrationBuilder.CreateIndex(
                name: "IX_NeedReviewHistory_ActorUserId",
                table: "NeedReviewHistory",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NeedReviewHistory_NeedId_OccurredAt",
                table: "NeedReviewHistory",
                columns: new[] { "NeedId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NeedReviewHistory");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RequestNeed_Category",
                table: "RequestNeed");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Request_Status",
                table: "AssistanceRequest");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "RequestNeed");

            migrationBuilder.DropColumn(
                name: "ServiceSubtype",
                table: "RequestNeed");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Request_Status",
                table: "AssistanceRequest",
                sql: "\"Status\" IN ('Submitted', 'Clarification', 'Accepted', 'Rejected', 'Open', 'Completed', 'ClosedUnresolved', 'Split')");
        }
    }
}
