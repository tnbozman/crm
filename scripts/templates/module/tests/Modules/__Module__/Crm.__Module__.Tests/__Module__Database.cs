using Crm.__Module__.Persistence;
using Crm.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Crm.__Module__.Tests;

/// <summary>A fresh database with the __Module__ migrations applied, one per test class (real SQL Server, never mocked).</summary>
public sealed class __Module__Database : IAsyncLifetime
{
    private readonly SqlFixture sql = new();

    public string ConnectionString => sql.ConnectionString;

    internal __Module__DbContext CreateDbContext() =>
        new(__Module__Persistence.Configure(new DbContextOptionsBuilder<__Module__DbContext>(), ConnectionString).Options);

    public async ValueTask InitializeAsync()
    {
        await sql.InitializeAsync();
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public ValueTask DisposeAsync() => sql.DisposeAsync();

    /// <summary>Deletes every row, keeping the schema. Call at the start of a test that needs empty tables.</summary>
    public Task ResetAsync() => sql.ResetAsync();
}
