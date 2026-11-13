using Microsoft.EntityFrameworkCore;

namespace HelpingHands.Api.Tests;

internal static class RegistrationFailureFixture
{
    public static Task InstallAsync(HelpingHands.Api.Data.HelpingHandsDbContext database, string table)
    {
        var statement = table switch
        {
            "Beneficiary" => "CREATE OR REPLACE FUNCTION p109_fail_insert() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'P1-09 rollback test'; END $$; CREATE TRIGGER p109_fail_insert BEFORE INSERT ON \"Beneficiary\" FOR EACH ROW EXECUTE FUNCTION p109_fail_insert();",
            "RecipientMember" => "CREATE OR REPLACE FUNCTION p109_fail_insert() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'P1-09 rollback test'; END $$; CREATE TRIGGER p109_fail_insert BEFORE INSERT ON \"RecipientMember\" FOR EACH ROW EXECUTE FUNCTION p109_fail_insert();",
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        };
        return database.Database.ExecuteSqlRawAsync(statement);
    }

    public static Task RemoveAsync(HelpingHands.Api.Data.HelpingHandsDbContext database, string table)
    {
        var statement = table switch
        {
            "Beneficiary" => "DROP TRIGGER IF EXISTS p109_fail_insert ON \"Beneficiary\"; DROP FUNCTION IF EXISTS p109_fail_insert();",
            "RecipientMember" => "DROP TRIGGER IF EXISTS p109_fail_insert ON \"RecipientMember\"; DROP FUNCTION IF EXISTS p109_fail_insert();",
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        };
        return database.Database.ExecuteSqlRawAsync(statement);
    }
}
