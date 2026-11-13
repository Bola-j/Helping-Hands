using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpingHands.Api.Migrations
{
    /// <inheritdoc />
    public partial class ServiceCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceVersion", x => x.Id);
                    table.CheckConstraint("CK_ServiceVersion_Number", "\"VersionNumber\" > 0");
                    table.ForeignKey(
                        name: "FK_ServiceVersion_ServiceCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ServiceCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitStandard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitStandard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitStandard_ServiceVersion_ServiceVersionId",
                        column: x => x.ServiceVersionId,
                        principalTable: "ServiceVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StandardVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitStandardId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardVersion", x => x.Id);
                    table.CheckConstraint("CK_StandardVersion_Number", "\"VersionNumber\" > 0");
                    table.ForeignKey(
                        name: "FK_StandardVersion_UnitStandard_UnitStandardId",
                        column: x => x.UnitStandardId,
                        principalTable: "UnitStandard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StandardComponent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StandardVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardComponent", x => x.Id);
                    table.CheckConstraint("CK_StandardComponent_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_StandardComponent_StandardVersion_StandardVersionId",
                        column: x => x.StandardVersionId,
                        principalTable: "StandardVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategory_Code",
                table: "ServiceCategory",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceVersion_CategoryId_VersionNumber",
                table: "ServiceVersion",
                columns: new[] { "CategoryId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StandardComponent_StandardVersionId",
                table: "StandardComponent",
                column: "StandardVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardVersion_UnitStandardId_VersionNumber",
                table: "StandardVersion",
                columns: new[] { "UnitStandardId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitStandard_ServiceVersionId",
                table: "UnitStandard",
                column: "ServiceVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StandardComponent");

            migrationBuilder.DropTable(
                name: "StandardVersion");

            migrationBuilder.DropTable(
                name: "UnitStandard");

            migrationBuilder.DropTable(
                name: "ServiceVersion");

            migrationBuilder.DropTable(
                name: "ServiceCategory");
        }
    }
}
