using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence.Configurations;

internal sealed class WatchSpaceAnimeConfiguration : IEntityTypeConfiguration<WatchSpaceAnime>
{
    public void Configure(EntityTypeBuilder<WatchSpaceAnime> builder)
    {
        builder.ToTable("watch_space_anime");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => WatchSpaceAnimeId.From(value));

        builder.Property(a => a.WatchSpaceId)
            .HasColumnName("watch_space_id")
            .IsRequired();

        builder.Property(a => a.AniListMediaId)
            .HasColumnName("anilist_media_id")
            .IsRequired();

        builder.Property(a => a.PreferredTitle)
            .HasColumnName("preferred_title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.EpisodeCountSnapshot)
            .HasColumnName("episode_count_snapshot");

        builder.Property(a => a.CoverImageUrlSnapshot)
            .HasColumnName("cover_image_url_snapshot")
            .HasMaxLength(2000);

        builder.Property(a => a.Format)
            .HasColumnName("format")
            .HasMaxLength(50);

        builder.Property(a => a.Season)
            .HasColumnName("season")
            .HasMaxLength(20);

        builder.Property(a => a.SeasonYear)
            .HasColumnName("season_year");

        builder.Property(a => a.SharedStatus)
            .HasColumnName("shared_status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.SharedEpisodesWatched)
            .HasColumnName("shared_episodes_watched")
            .IsRequired();

        builder.Property(a => a.Mood)
            .HasColumnName("mood")
            .HasMaxLength(200);

        builder.Property(a => a.Vibe)
            .HasColumnName("vibe")
            .HasMaxLength(200);

        builder.Property(a => a.Pitch)
            .HasColumnName("pitch")
            .HasMaxLength(500);

        builder.Property(a => a.AddedByUserId)
            .HasColumnName("added_by_user_id")
            .IsRequired();

        builder.Property(a => a.AddedAtUtc)
            .HasColumnName("added_at_utc")
            .IsRequired();

        // Unique constraint: one anime per watch space
        builder.HasIndex(a => new { a.WatchSpaceId, a.AniListMediaId })
            .IsUnique()
            .HasDatabaseName("ix_watch_space_anime_space_media");

        // Relationship to ParticipantEntries
        builder.HasMany(a => a.ParticipantEntries)
            .WithOne()
            .HasForeignKey(p => p.WatchSpaceAnimeId)
            .HasPrincipalKey(a => a.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.ParticipantEntries)
            .HasField("_participantEntries")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
