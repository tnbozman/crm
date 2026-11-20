using Crm.Migrator.Tests.Probe;
using Crm.SharedKernel.Modules;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Crm.Migrator.Tests;

public sealed class MigrationRunnerTests
{
    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    [InlineData("Test")]
    public async Task RunAsync_ResetOutsideDevelopment_IsRefused(string environment)
    {
        // The server is never contacted: the refusal comes first.
        var configuration = Configuration("Server=never-contacted;Database=crm");

        var refusal = await Should.ThrowAsync<InvalidOperationException>(() =>
            MigrationRunner.RunAsync([new ProbeModule()], configuration, environment, reset: true, TestContext.Current.CancellationToken));

        refusal.Message.ShouldContain("Development");
    }

    [Fact]
    public async Task RunAsync_WithoutConnectionString_Throws()
    {
        var refusal = await Should.ThrowAsync<InvalidOperationException>(() =>
            MigrationRunner.RunAsync([new ProbeModule()], Configuration(null), "Development", reset: false, TestContext.Current.CancellationToken));

        refusal.Message.ShouldContain($"ConnectionStrings:{ModuleDatabase.ConnectionStringName}");
    }

    internal static IConfiguration Configuration(string? connectionString) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [$"ConnectionStrings:{ModuleDatabase.ConnectionStringName}"] = connectionString })
            .Build();
}
