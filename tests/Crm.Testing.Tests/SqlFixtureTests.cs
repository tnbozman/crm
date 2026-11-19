using Crm.Testing;
using Microsoft.Data.SqlClient;
using Shouldly;
using Xunit;

namespace Crm.Testing.Tests;

[Trait("Category", TestCategories.Integration)]
public sealed class SqlFixtureTests
{
    [Fact]
    public async Task SqlFixture_CreatesIsolatedDatabase_AndDropsIt()
    {
        var first = new SqlFixture();
        var second = new SqlFixture();

        // Two test classes initialising their fixtures in parallel.
        await Task.WhenAll(first.InitializeAsync().AsTask(), second.InitializeAsync().AsTask());

        first.DatabaseName.ShouldNotBe(second.DatabaseName);
        (await DatabaseExistsAsync(first.DatabaseName)).ShouldBeTrue();
        (await DatabaseExistsAsync(second.DatabaseName)).ShouldBeTrue();

        await ExecuteAsync(first.ConnectionString, "CREATE TABLE dbo.Probe (Id int NOT NULL); INSERT dbo.Probe VALUES (1);");
        (await ScalarAsync(second.ConnectionString, "SELECT OBJECT_ID('dbo.Probe')")).ShouldBe(DBNull.Value);

        await Task.WhenAll(first.DisposeAsync().AsTask(), second.DisposeAsync().AsTask());

        (await DatabaseExistsAsync(first.DatabaseName)).ShouldBeFalse();
        (await DatabaseExistsAsync(second.DatabaseName)).ShouldBeFalse();
    }

    private static async Task<bool> DatabaseExistsAsync(string name) =>
        await ScalarAsync(SqlFixture.MasterConnectionString(), $"SELECT DB_ID(N'{name}')") is not DBNull;

    private static async Task<object> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new SqlCommand(sql, connection);
        return await command.ExecuteScalarAsync(TestContext.Current.CancellationToken) ?? DBNull.Value;
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }
}
