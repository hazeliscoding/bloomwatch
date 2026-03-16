using BloomWatch.Modules.AniListSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.AniListSync.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the Entity Framework Core mapping for the <see cref="MediaCacheEntry"/> entity.
/// </summary>
internal sealed class MediaCacheEntryConfiguration : IEntityTypeConfiguration<MediaCacheEntry>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MediaCacheEntry> builder)
    {
        builder.ToTable("media_cache");

        builder.HasKey(e => e.AnilistMediaId);

        builder.Property(e => e.AnilistMediaId)
            .HasColumnName("anilist_media_id")
            .ValueGeneratedNever();

        builder.Property(e => e.TitleRomaji)
            .HasColumnName("title_romaji")
            .HasMaxLength(500);

        builder.Property(e => e.TitleEnglish)
            .HasColumnName("title_english")
            .HasMaxLength(500);

        builder.Property(e => e.TitleNative)
            .HasColumnName("title_native")
            .HasMaxLength(500);

        builder.Property(e => e.CoverImageUrl)
            .HasColumnName("cover_image_url")
            .HasMaxLength(2000);

        builder.Property(e => e.Episodes)
            .HasColumnName("episodes");

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(50);

        builder.Property(e => e.Format)
            .HasColumnName("format")
            .HasMaxLength(50);

        builder.Property(e => e.Season)
            .HasColumnName("season")
            .HasMaxLength(50);

        builder.Property(e => e.SeasonYear)
            .HasColumnName("season_year");

        builder.Property(e => e.Genres)
            .HasColumnName("genres")
            .HasColumnType("jsonb");

        builder.Property(e => e.Description)
            .HasColumnName("description");

        builder.Property(e => e.AverageScore)
            .HasColumnName("average_score");

        builder.Property(e => e.Popularity)
            .HasColumnName("popularity");

        builder.Property(e => e.CachedAt)
            .HasColumnName("cached_at")
            .IsRequired();
    }
}
