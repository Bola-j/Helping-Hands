using HelpingHands.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HelpingHands.Api.Tests;

public sealed partial class WorkflowDbTests
{
    private static async Task AssertInsertFailsAsync(HelpingHandsDbContext db, object entity, string sqlState)
    {
        db.Add(entity);
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(sqlState, Assert.IsType<PostgresException>(error.InnerException).SqlState);
        db.ChangeTracker.Clear();
    }
}
