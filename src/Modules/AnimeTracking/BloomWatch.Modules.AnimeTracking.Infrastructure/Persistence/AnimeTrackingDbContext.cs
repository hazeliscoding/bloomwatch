using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Entities;
using BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the AnimeTracking module, targeting the <c>anime_tracking</c> schema.
/// </summary>
public sealed class AnimeTrackingDbContext(DbContextOptions<AnimeTrackingDbContext> options) : DbContext(options)
{
    public DbSet<WatchSpaceAnime> WatchSpaceAnimes => Set<WatchSpaceAnime>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("anime_tracking");
        modelBuilder.ApplyConfiguration(new WatchSpaceAnimeConfiguration());
        modelBuilder.ApplyConfiguration(new ParticipantEntryConfiguration());
        modelBuilder.ApplyConfiguration(new WatchSessionConfiguration());
    }
}
