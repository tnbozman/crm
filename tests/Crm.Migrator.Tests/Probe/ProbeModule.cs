using System.Reflection;
using Crm.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Migrator.Tests.Probe;

/// <summary>A test-only module with one migration (probe.Probes), registered the way scaffolded modules register theirs.</summary>
internal sealed class ProbeModule : IModule
{
    public string Name => "Probe";

    public Assembly ControllersAssembly => typeof(ProbeModule).Assembly;

    public void AddServices(IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContext<ProbeDbContext>(options => options.UseSqlServer(
            configuration.GetConnectionString(ModuleDatabase.ConnectionStringName),
            sql => sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "probe")));
}

internal sealed class ProbeDbContext(DbContextOptions<ProbeDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.HasDefaultSchema("probe");
}

[DbContext(typeof(ProbeDbContext))]
[Migration("20261214000000_CreateProbes")]
internal sealed class CreateProbes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("CREATE TABLE probe.Probes (Id int NOT NULL PRIMARY KEY);");
}
