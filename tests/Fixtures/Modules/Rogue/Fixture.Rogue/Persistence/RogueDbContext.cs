using Microsoft.EntityFrameworkCore;

namespace Fixture.Rogue.Persistence;

internal sealed class RogueDbContext(DbContextOptions<RogueDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.HasDefaultSchema("rogue");
}
