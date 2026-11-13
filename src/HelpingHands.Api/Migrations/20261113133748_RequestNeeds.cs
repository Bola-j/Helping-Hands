using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpingHands.Api.Migrations
{
    /// <inheritdoc />
    public partial class RequestNeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMultiNeed",
                table: "AssistanceRequest",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RequestNeed",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    StandardVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestNeed", x => x.Id);
                    table.CheckConstraint("CK_RequestNeed_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_RequestNeed_AssistanceRequest_RequestId",
                        column: x => x.RequestId,
                        principalTable: "AssistanceRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestNeed_StandardVersion_StandardVersionId",
                        column: x => x.StandardVersionId,
                        principalTable: "StandardVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Request_DemandMode",
                table: "AssistanceRequest",
                sql: "NOT \"IsMultiNeed\" OR (\"Category\" IS NULL AND \"ServiceSubtype\" IS NULL AND \"RequiredQuantity\" IS NULL AND \"Unit\" IS NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_RequestNeed_RequestId_StandardVersionId",
                table: "RequestNeed",
                columns: new[] { "RequestId", "StandardVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestNeed_StandardVersionId",
                table: "RequestNeed",
                column: "StandardVersionId");
            migrationBuilder.Sql("""
                CREATE FUNCTION guard_request_mode() RETURNS trigger LANGUAGE plpgsql AS $body$
                BEGIN
                    IF NEW."IsMultiNeed" <> OLD."IsMultiNeed" THEN RAISE EXCEPTION 'Request demand mode cannot change'; END IF;
                    RETURN NEW;
                END $body$;
                CREATE TRIGGER guard_request_mode BEFORE UPDATE ON "AssistanceRequest" FOR EACH ROW EXECUTE FUNCTION guard_request_mode();
                CREATE FUNCTION guard_request_need() RETURNS trigger LANGUAGE plpgsql AS $body$
                DECLARE multi_need boolean; published boolean;
                BEGIN
                    IF TG_OP = 'UPDATE' AND (NEW."RequestId" <> OLD."RequestId" OR NEW."StandardVersionId" <> OLD."StandardVersionId") THEN
                        RAISE EXCEPTION 'A need cannot change its request or recorded standard version';
                    END IF;
                    SELECT "IsMultiNeed" INTO multi_need FROM "AssistanceRequest" WHERE "Id" = NEW."RequestId" FOR UPDATE;
                    IF multi_need IS DISTINCT FROM TRUE THEN RAISE EXCEPTION 'Needs require a multi-need request'; END IF;
                    SELECT sv."IsPublished" AND service."IsPublished" INTO published
                    FROM "StandardVersion" sv JOIN "UnitStandard" u ON u."Id" = sv."UnitStandardId"
                    JOIN "ServiceVersion" service ON service."Id" = u."ServiceVersionId" WHERE sv."Id" = NEW."StandardVersionId";
                    IF published IS DISTINCT FROM TRUE THEN RAISE EXCEPTION 'Needs require published service and standard versions'; END IF;
                    RETURN NEW;
                END $body$;
                CREATE TRIGGER guard_request_need BEFORE INSERT OR UPDATE ON "RequestNeed" FOR EACH ROW EXECUTE FUNCTION guard_request_need();
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER guard_request_need ON "RequestNeed";
                DROP FUNCTION guard_request_need();
                DROP TRIGGER guard_request_mode ON "AssistanceRequest";
                DROP FUNCTION guard_request_mode();
                """);

            migrationBuilder.DropTable(
                name: "RequestNeed");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Request_DemandMode",
                table: "AssistanceRequest");

            migrationBuilder.DropColumn(
                name: "IsMultiNeed",
                table: "AssistanceRequest");
        }
    }
}
