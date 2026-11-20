using Crm.__Module__.Persistence;
using Crm.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Crm.__Module__.Tests.Persistence;

public sealed class ModelTests
{
    [Fact]
    public void Model_HasNoPendingChanges_AgainstTheMigrations()
    {
        using var db = new __Module__DbContext(
            __Module__Persistence.Configure(new DbContextOptionsBuilder<__Module__DbContext>(), "Server=model-only;Database=none").Options);

        db.Database.HasPendingModelChanges().ShouldBeFalse("run make migration-add module=__Module__ name=<Change>");
    }
}

[Trait("Category", TestCategories.Integration)]
public sealed class MigrationsTests(__Module__Database database) : IClassFixture<__Module__Database>
{
    [Fact]
    public async Task Migrations_ApplyToEmptyDatabase_InTheModuleSchemaOnly()
    {
        await using var connection = new SqlConnection(database.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new SqlCommand(
            "SELECT DISTINCT s.name FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id", connection);
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        var schemas = new List<string>();
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            schemas.Add(reader.GetString(0));
        }

        schemas.ShouldBe([__Module__DbContext.Schema], "every table, the migrations history included, is in the module's schema");
    }
}
