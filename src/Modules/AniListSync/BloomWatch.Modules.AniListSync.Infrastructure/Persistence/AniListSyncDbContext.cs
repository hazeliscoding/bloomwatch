using BloomWatch.Modules.AniListSync.Domain.Entities;
using BloomWatch.Modules.AniListSync.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.AniListSync.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core <see cref="DbContext"/> for the AniListSync module.
/// </summary>
/// <remarks>
/// All tables are placed in the <c>anilist_sync</c> database schema to isolate data from other
/// modules in the same physical database (schema-per-module strategy).
/// </remarks>
public sealed class AniListSyncDbContext(DbContextOptions<AniListSyncDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> for <see cref="MediaCacheEntry"/> entities.
    /// </summary>
    public DbSet<MediaCacheEntry> MediaCacheEntries => Set<MediaCacheEntry>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("anilist_sync");
        modelBuilder.ApplyConfiguration(new MediaCacheEntryConfiguration());
    }
}
