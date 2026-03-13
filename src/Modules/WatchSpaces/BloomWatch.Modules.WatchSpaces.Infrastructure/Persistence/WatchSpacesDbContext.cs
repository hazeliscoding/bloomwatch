using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Persistence;

public sealed class WatchSpacesDbContext(DbContextOptions<WatchSpacesDbContext> options) : DbContext(options)
{
    public DbSet<WatchSpace> WatchSpaces => Set<WatchSpace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("watch_spaces");
        modelBuilder.ApplyConfiguration(new WatchSpaceConfiguration());
        modelBuilder.ApplyConfiguration(new WatchSpaceMemberConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationConfiguration());
    }
}
