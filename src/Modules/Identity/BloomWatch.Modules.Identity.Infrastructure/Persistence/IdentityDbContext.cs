using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
