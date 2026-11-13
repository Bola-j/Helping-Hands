using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpingHands.Api.Migrations
{
    /// <inheritdoc />
    public partial class PreservePublishedCatalogVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "StandardVersion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "ServiceVersion",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            migrationBuilder.Sql("""
                UPDATE "ServiceVersion" SET "IsPublished" = TRUE;
                UPDATE "StandardVersion" SET "IsPublished" = TRUE;

                CREATE FUNCTION catalog_guard_service_version() RETURNS trigger LANGUAGE plpgsql AS $body$
                BEGIN
                    IF TG_OP = 'INSERT' THEN
                        IF NEW."IsPublished" THEN RAISE EXCEPTION 'Create service versions as drafts'; END IF;
                        RETURN NEW;
                    END IF;
                    IF OLD."IsPublished" THEN RAISE EXCEPTION 'Published service versions are immutable'; END IF;
                    IF TG_OP = 'DELETE' THEN RETURN OLD; END IF;
                    IF NEW."IsPublished" THEN
                        PERFORM 1 FROM "ServiceCategory" WHERE "Id" = NEW."CategoryId" FOR UPDATE;
                    END IF;
                    RETURN NEW;
                END $body$;
                CREATE TRIGGER guard_service_version BEFORE INSERT OR UPDATE OR DELETE ON "ServiceVersion"
                    FOR EACH ROW EXECUTE FUNCTION catalog_guard_service_version();

                CREATE FUNCTION catalog_guard_category() RETURNS trigger LANGUAGE plpgsql AS $body$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "ServiceVersion" WHERE "CategoryId" = OLD."Id" AND "IsPublished") THEN
                        RAISE EXCEPTION 'Category with a published version is immutable';
                    END IF;
                    IF TG_OP = 'DELETE' THEN RETURN OLD; END IF;
                    RETURN NEW;
                END $body$;
                CREATE TRIGGER guard_category BEFORE UPDATE OR DELETE ON "ServiceCategory"
                    FOR EACH ROW EXECUTE FUNCTION catalog_guard_category();

                CREATE FUNCTION catalog_guard_standard_version() RETURNS trigger LANGUAGE plpgsql AS $body$
                DECLARE service_published boolean;
                BEGIN
                    IF TG_OP = 'INSERT' THEN
                        IF NEW."IsPublished" THEN RAISE EXCEPTION 'Create standard versions as drafts'; END IF;
                        RETURN NEW;
                    END IF;
                    IF OLD."IsPublished" THEN RAISE EXCEPTION 'Published standard versions are immutable'; END IF;
                    IF TG_OP = 'DELETE' THEN RETURN OLD; END IF;
                    IF NEW."IsPublished" THEN
                        SELECT sv."IsPublished" INTO service_published
                        FROM "UnitStandard" us JOIN "ServiceVersion" sv ON sv."Id" = us."ServiceVersionId"
                        WHERE us."Id" = NEW."UnitStandardId" FOR UPDATE OF us, sv;
                        IF service_published IS DISTINCT FROM TRUE THEN
                            RAISE EXCEPTION 'Standard requires a published service version';
                        END IF;
                    END IF;
                    RETURN NEW;
                END $body$;
                CREATE TRIGGER guard_standard_version BEFORE INSERT OR UPDATE OR DELETE ON "StandardVersion"
                    FOR EACH ROW EXECUTE FUNCTION catalog_guard_standard_version();

                CREATE FUNCTION catalog_guard_unit_standard() RETURNS trigger LANGUAGE plpgsql AS $body$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "StandardVersion" WHERE "UnitStandardId" = OLD."Id" AND "IsPublished") THEN
                        RAISE EXCEPTION 'Standard with a published version is immutable';
                    END IF;
                    IF TG_OP = 'DELETE' THEN RETURN OLD; END IF;
                    RETURN NEW;
                END $body$;
                CREATE TRIGGER guard_unit_standard BEFORE UPDATE OR DELETE ON "UnitStandard"
                    FOR EACH ROW EXECUTE FUNCTION catalog_guard_unit_standard();

                CREATE FUNCTION catalog_guard_component() RETURNS trigger LANGUAGE plpgsql AS $body$
                DECLARE old_id uuid; new_id uuid; version_row record;
                BEGIN
                    IF TG_OP <> 'INSERT' THEN old_id := OLD."StandardVersionId"; END IF;
                    IF TG_OP <> 'DELETE' THEN new_id := NEW."StandardVersionId"; END IF;
                    FOR version_row IN SELECT "IsPublished" FROM "StandardVersion"
                        WHERE "Id" IN (old_id, new_id) ORDER BY "Id" FOR UPDATE
                    LOOP
                        IF version_row."IsPublished" THEN
                            RAISE EXCEPTION 'Published standard components are immutable';
                        END IF;
                    END LOOP;
                    IF TG_OP = 'DELETE' THEN RETURN OLD; END IF;
                    RETURN NEW;
                END $body$;
                CREATE TRIGGER guard_component BEFORE INSERT OR UPDATE OR DELETE ON "StandardComponent"
                    FOR EACH ROW EXECUTE FUNCTION catalog_guard_component();

                CREATE FUNCTION catalog_guard_truncate() RETURNS trigger LANGUAGE plpgsql AS $body$
                BEGIN
                    RAISE EXCEPTION 'Catalog tables cannot be truncated';
                END $body$;
                CREATE TRIGGER guard_category_truncate BEFORE TRUNCATE ON "ServiceCategory" FOR EACH STATEMENT EXECUTE FUNCTION catalog_guard_truncate();
                CREATE TRIGGER guard_service_truncate BEFORE TRUNCATE ON "ServiceVersion" FOR EACH STATEMENT EXECUTE FUNCTION catalog_guard_truncate();
                CREATE TRIGGER guard_unit_truncate BEFORE TRUNCATE ON "UnitStandard" FOR EACH STATEMENT EXECUTE FUNCTION catalog_guard_truncate();
                CREATE TRIGGER guard_version_truncate BEFORE TRUNCATE ON "StandardVersion" FOR EACH STATEMENT EXECUTE FUNCTION catalog_guard_truncate();
                CREATE TRIGGER guard_component_truncate BEFORE TRUNCATE ON "StandardComponent" FOR EACH STATEMENT EXECUTE FUNCTION catalog_guard_truncate();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER guard_category_truncate ON "ServiceCategory";
                DROP TRIGGER guard_service_truncate ON "ServiceVersion";
                DROP TRIGGER guard_unit_truncate ON "UnitStandard";
                DROP TRIGGER guard_version_truncate ON "StandardVersion";
                DROP TRIGGER guard_component_truncate ON "StandardComponent";
                DROP TRIGGER guard_service_version ON "ServiceVersion";
                DROP TRIGGER guard_category ON "ServiceCategory";
                DROP TRIGGER guard_standard_version ON "StandardVersion";
                DROP TRIGGER guard_unit_standard ON "UnitStandard";
                DROP TRIGGER guard_component ON "StandardComponent";
                DROP FUNCTION catalog_guard_truncate();
                DROP FUNCTION catalog_guard_service_version();
                DROP FUNCTION catalog_guard_category();
                DROP FUNCTION catalog_guard_standard_version();
                DROP FUNCTION catalog_guard_unit_standard();
                DROP FUNCTION catalog_guard_component();
                """);
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "StandardVersion");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "ServiceVersion");
        }
    }
}
