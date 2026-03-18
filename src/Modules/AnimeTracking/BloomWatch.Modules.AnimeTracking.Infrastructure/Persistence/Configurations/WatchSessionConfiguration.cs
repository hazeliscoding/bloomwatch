using BloomWatch.Modules.AnimeTracking.Domain.Entities;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomWatch.Modules.AnimeTracking.Infrastructure.Persistence.Configurations;

internal sealed class WatchSessionConfiguration : IEntityTypeConfiguration<WatchSession>
{
    public void Configure(EntityTypeBuilder<WatchSession> builder)
    {
        builder.ToTable("watch_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.WatchSpaceAnimeId)
            .HasColumnName("watch_space_anime_id")
            .HasConversion(id => id.Value, value => WatchSpaceAnimeId.From(value))
            .IsRequired();

        builder.Property(s => s.SessionDateUtc)
            .HasColumnName("session_date_utc")
            .IsRequired();

        builder.Property(s => s.StartEpisode)
            .HasColumnName("start_episode")
            .IsRequired();

        builder.Property(s => s.EndEpisode)
            .HasColumnName("end_episode")
            .IsRequired();

        builder.Property(s => s.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(s => s.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.HasIndex(s => s.WatchSpaceAnimeId)
            .HasDatabaseName("ix_watch_sessions_anime");
    }
}
