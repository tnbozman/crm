using Microsoft.EntityFrameworkCore;

namespace Fixture.Rogue.Persistence;

/// <summary>Violations: a second DbContext in the module, with a schema other than the module's.</summary>
internal sealed class RogueReportingDbContext(DbContextOptions<RogueReportingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.HasDefaultSchema("reporting");
}
