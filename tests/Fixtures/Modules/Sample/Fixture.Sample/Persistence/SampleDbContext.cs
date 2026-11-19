using Microsoft.EntityFrameworkCore;

namespace Fixture.Sample.Persistence;

internal sealed class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.HasDefaultSchema("sample");
}
