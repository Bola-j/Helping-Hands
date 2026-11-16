using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpingHands.Api.Migrations
{
    /// <inheritdoc />
    public partial class CharityServicesAndDailyCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CharityService",
                columns: table => new
                {
                    CharityServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StandardVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharityService", x => x.CharityServiceId);
                    table.ForeignKey(
                        name: "FK_CharityService_Charity_CharityId",
                        column: x => x.CharityId,
                        principalTable: "Charity",
                        principalColumn: "RecipientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CharityService_ServiceVersion_ServiceVersionId",
                        column: x => x.ServiceVersionId,
                        principalTable: "ServiceVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CharityService_StandardVersion_StandardVersionId",
                        column: x => x.StandardVersionId,
                        principalTable: "StandardVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyCapacity",
                columns: table => new
                {
                    CapacityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharityServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CairoDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Available = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCapacity", x => x.CapacityId);
                    table.CheckConstraint("CK_DailyCapacity_Available_NonNegative", "\"Available\" >= 0");
                    table.ForeignKey(
                        name: "FK_DailyCapacity_CharityService_CharityServiceId",
                        column: x => x.CharityServiceId,
                        principalTable: "CharityService",
                        principalColumn: "CharityServiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CapacityUpdate",
                columns: table => new
                {
                    CapacityUpdateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapacityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Available = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapacityUpdate", x => x.CapacityUpdateId);
                    table.CheckConstraint("CK_CapacityUpdate_Available_NonNegative", "\"Available\" >= 0");
                    table.ForeignKey(
                        name: "FK_CapacityUpdate_DailyCapacity_CapacityId",
                        column: x => x.CapacityId,
                        principalTable: "DailyCapacity",
                        principalColumn: "CapacityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CapacityUpdate_UserAccount_ActorId",
                        column: x => x.ActorId,
                        principalTable: "UserAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapacityUpdate_ActorId",
                table: "CapacityUpdate",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_CapacityUpdate_CapacityId_UpdatedAt",
                table: "CapacityUpdate",
                columns: new[] { "CapacityId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CharityService_CharityId_ServiceVersionId_StandardVersionId",
                table: "CharityService",
                columns: new[] { "CharityId", "ServiceVersionId", "StandardVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharityService_ServiceVersionId",
                table: "CharityService",
                column: "ServiceVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CharityService_StandardVersionId",
                table: "CharityService",
                column: "StandardVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyCapacity_CharityServiceId_CairoDate",
                table: "DailyCapacity",
                columns: new[] { "CharityServiceId", "CairoDate" },
                unique: true);

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION fn_enforce_charity_service_version_match()
                RETURNS trigger AS $$
                DECLARE
                    v_service_id uuid;
                    v_standard_service_id uuid;
                BEGIN
                    SELECT "ServiceId" INTO v_service_id FROM "ServiceVersion" WHERE "Id" = NEW."ServiceVersionId";
                    SELECT "ServiceId" INTO v_standard_service_id FROM "StandardVersion" WHERE "Id" = NEW."StandardVersionId";

                    IF v_service_id IS NULL OR v_standard_service_id IS NULL OR v_service_id <> v_standard_service_id THEN
                        RAISE EXCEPTION 'ServiceVersionId (%) and StandardVersionId (%) must belong to the same Service', NEW."ServiceVersionId", NEW."StandardVersionId";
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_charity_service_version_match
                BEFORE INSERT OR UPDATE ON "CharityService"
                FOR EACH ROW
                EXECUTE FUNCTION fn_enforce_charity_service_version_match();

                CREATE OR REPLACE FUNCTION fn_enforce_capacity_update_append_only()
                RETURNS trigger AS $$
                BEGIN
                    RAISE EXCEPTION 'CapacityUpdate is append-only; UPDATE and DELETE are prohibited';
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_capacity_update_append_only
                BEFORE UPDATE OR DELETE ON "CapacityUpdate"
                FOR EACH ROW
                EXECUTE FUNCTION fn_enforce_capacity_update_append_only();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_capacity_update_append_only ON "CapacityUpdate";
                DROP FUNCTION IF EXISTS fn_enforce_capacity_update_append_only();
                DROP TRIGGER IF EXISTS trg_charity_service_version_match ON "CharityService";
                DROP FUNCTION IF EXISTS fn_enforce_charity_service_version_match();
                """);

            migrationBuilder.DropTable(
                name: "CapacityUpdate");

            migrationBuilder.DropTable(
                name: "DailyCapacity");

            migrationBuilder.DropTable(
                name: "CharityService");
        }
    }
}