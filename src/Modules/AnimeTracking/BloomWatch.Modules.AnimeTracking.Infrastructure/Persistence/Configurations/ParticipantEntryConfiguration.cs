using BloomWatch.Modules.AnimeTracking.Domain.Entities;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence.Configurations;

internal sealed class ParticipantEntryConfiguration : IEntityTypeConfiguration<ParticipantEntry>
{
    public void Configure(EntityTypeBuilder<ParticipantEntry> builder)
    {
        builder.ToTable("participant_entries");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.WatchSpaceAnimeId)
            .HasColumnName("watch_space_anime_id")
            .HasConversion(id => id.Value, value => WatchSpaceAnimeId.From(value))
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.IndividualStatus)
            .HasColumnName("individual_status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.EpisodesWatched)
            .HasColumnName("episodes_watched")
            .IsRequired();

        builder.Property(p => p.RatingScore)
            .HasColumnName("rating_score")
            .HasPrecision(3, 1);

        builder.Property(p => p.RatingNotes)
            .HasColumnName("rating_notes")
            .HasMaxLength(1000);

        builder.Property(p => p.LastUpdatedAtUtc)
            .HasColumnName("last_updated_at_utc")
            .IsRequired();

        builder.HasIndex(p => new { p.WatchSpaceAnimeId, p.UserId })
            .IsUnique()
            .HasDatabaseName("ix_participant_entries_anime_user");
    }
}
