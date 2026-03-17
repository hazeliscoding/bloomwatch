using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.CrossModule;

/// <summary>
/// Minimal read-only context projecting the <c>anilist_sync.media_cache</c> table
/// for media metadata lookups.
/// </summary>
public sealed class AniListMediaCacheReadDbContext(
    DbContextOptions<AniListMediaCacheReadDbContext> options) : DbContext(options)
{
    public DbSet<MediaCacheRow> MediaCache => Set<MediaCacheRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaCacheRow>(entity =>
        {
            entity.ToTable("media_cache", "anilist_sync");
            entity.HasKey(m => m.AnilistMediaId);
            entity.Property(m => m.AnilistMediaId).HasColumnName("anilist_media_id");
            entity.Property(m => m.TitleEnglish).HasColumnName("title_english");
            entity.Property(m => m.TitleRomaji).HasColumnName("title_romaji");
            entity.Property(m => m.TitleNative).HasColumnName("title_native");
            entity.Property(m => m.CoverImageUrl).HasColumnName("cover_image_url");
            entity.Property(m => m.Episodes).HasColumnName("episodes");
            entity.Property(m => m.Format).HasColumnName("format");
            entity.Property(m => m.Season).HasColumnName("season");
            entity.Property(m => m.SeasonYear).HasColumnName("season_year");
        });
    }
}

/// <summary>
/// Read-only projection of media_cache for cross-module metadata lookups.
/// </summary>
public sealed class MediaCacheRow
{
    public int AnilistMediaId { get; set; }
    public string? TitleEnglish { get; set; }
    public string? TitleRomaji { get; set; }
    public string? TitleNative { get; set; }
    public string? CoverImageUrl { get; set; }
    public int? Episodes { get; set; }
    public string? Format { get; set; }
    public string? Season { get; set; }
    public int? SeasonYear { get; set; }
}
