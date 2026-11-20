using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpingHands.Api.Migrations
{
    /// <inheritdoc />
    public partial class LegacyRequestStandardMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LegacyMappedAt",
                table: "AssistanceRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LegacyMappingActorId",
                table: "AssistanceRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LegacyStandardVersionId",
                table: "AssistanceRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequest_LegacyMappingActorId",
                table: "AssistanceRequest",
                column: "LegacyMappingActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceRequest_LegacyStandardVersionId",
                table: "AssistanceRequest",
                column: "LegacyStandardVersionId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Request_LegacyMapping",
                table: "AssistanceRequest",
                sql: "(\"LegacyStandardVersionId\" IS NULL AND \"LegacyMappingActorId\" IS NULL AND \"LegacyMappedAt\" IS NULL) OR (NOT \"IsMultiNeed\" AND \"LegacyStandardVersionId\" IS NOT NULL AND \"LegacyMappingActorId\" IS NOT NULL AND \"LegacyMappedAt\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_AssistanceRequest_StandardVersion_LegacyStandardVersionId",
                table: "AssistanceRequest",
                column: "LegacyStandardVersionId",
                principalTable: "StandardVersion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssistanceRequest_UserAccount_LegacyMappingActorId",
                table: "AssistanceRequest",
                column: "LegacyMappingActorId",
                principalTable: "UserAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssistanceRequest_StandardVersion_LegacyStandardVersionId",
                table: "AssistanceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_AssistanceRequest_UserAccount_LegacyMappingActorId",
                table: "AssistanceRequest");

            migrationBuilder.DropIndex(
                name: "IX_AssistanceRequest_LegacyMappingActorId",
                table: "AssistanceRequest");

            migrationBuilder.DropIndex(
                name: "IX_AssistanceRequest_LegacyStandardVersionId",
                table: "AssistanceRequest");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Request_LegacyMapping",
                table: "AssistanceRequest");

            migrationBuilder.DropColumn(
                name: "LegacyMappedAt",
                table: "AssistanceRequest");

            migrationBuilder.DropColumn(
                name: "LegacyMappingActorId",
                table: "AssistanceRequest");

            migrationBuilder.DropColumn(
                name: "LegacyStandardVersionId",
                table: "AssistanceRequest");
        }
    }
}
