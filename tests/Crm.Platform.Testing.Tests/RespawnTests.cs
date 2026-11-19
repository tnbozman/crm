using Microsoft.Data.SqlClient;
using Shouldly;
using Xunit;

namespace Crm.Platform.Testing.Tests;

[Trait("Category", TestCategories.Integration)]
public sealed class RespawnTests(SqlFixture sql) : IClassFixture<SqlFixture>
{
    [Fact]
    public async Task Respawn_ResetsData_BetweenTests()
    {
        await ExecuteAsync("CREATE TABLE dbo.Sample (Id int NOT NULL PRIMARY KEY); INSERT dbo.Sample VALUES (1), (2);");

        await sql.ResetAsync();

        (await CountAsync()).ShouldBe(0);
    }

    private async Task<int> CountAsync()
    {
        await using var connection = new SqlConnection(sql.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Sample", connection);
        return (int)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))!;
    }

    private async Task ExecuteAsync(string sqlText)
    {
        await using var connection = new SqlConnection(sql.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new SqlCommand(sqlText, connection);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }
}
