using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpingHands.Api.Migrations
{
    /// <inheritdoc />
    public partial class AssociateReferralsWithNeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NeedId",
                table: "Referral",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_RequestNeed_RequestId_Id",
                table: "RequestNeed",
                columns: new[] { "RequestId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Referral_NeedId",
                table: "Referral",
                column: "NeedId");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_RequestId_NeedId",
                table: "Referral",
                columns: new[] { "RequestId", "NeedId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Referral_RequestNeed_RequestId_NeedId",
                table: "Referral",
                columns: new[] { "RequestId", "NeedId" },
                principalTable: "RequestNeed",
                principalColumns: new[] { "RequestId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referral_RequestNeed_RequestId_NeedId",
                table: "Referral");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_RequestNeed_RequestId_Id",
                table: "RequestNeed");

            migrationBuilder.DropIndex(
                name: "IX_Referral_NeedId",
                table: "Referral");

            migrationBuilder.DropIndex(
                name: "IX_Referral_RequestId_NeedId",
                table: "Referral");

            migrationBuilder.DropColumn(
                name: "NeedId",
                table: "Referral");
        }
    }
}
