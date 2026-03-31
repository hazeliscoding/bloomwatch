using Microsoft.EntityFrameworkCore;

namespace BloomWatch.Modules.Analytics.Infrastructure.CrossModule;

/// <summary>
/// Minimal read-only context projecting the <c>anime_tracking</c> schema tables
/// for analytics queries.
/// </summary>
public sealed class AnimeTrackingReadDbContext(
    DbContextOptions<AnimeTrackingReadDbContext> options) : DbContext(options)
{
    public DbSet<AnimeRow> Anime => Set<AnimeRow>();
    public DbSet<ParticipantRow> Participants => Set<ParticipantRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnimeRow>(entity =>
        {
            entity.ToTable("watch_space_anime", "anime_tracking");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.WatchSpaceId).HasColumnName("watch_space_id");
            entity.Property(a => a.PreferredTitle).HasColumnName("preferred_title");
            entity.Property(a => a.CoverImageUrlSnapshot).HasColumnName("cover_image_url_snapshot");
            entity.Property(a => a.EpisodeCountSnapshot).HasColumnName("episode_count_snapshot");
            entity.Property(a => a.Format).HasColumnName("format");
            entity.Property(a => a.SharedStatus).HasColumnName("shared_status");
            entity.Property(a => a.SharedEpisodesWatched).HasColumnName("shared_episodes_watched");
            entity.Property(a => a.AddedAtUtc).HasColumnName("added_at_utc");
            entity.Property(a => a.Mood).HasColumnName("mood");
            entity.Property(a => a.Vibe).HasColumnName("vibe");
            entity.Property(a => a.Pitch).HasColumnName("pitch");

            entity.HasMany(a => a.Participants)
                .WithOne()
                .HasForeignKey(p => p.WatchSpaceAnimeId);
        });

        modelBuilder.Entity<ParticipantRow>(entity =>
        {
            entity.ToTable("participant_entries", "anime_tracking");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.WatchSpaceAnimeId).HasColumnName("watch_space_anime_id");
            entity.Property(p => p.UserId).HasColumnName("user_id");
            entity.Property(p => p.RatingScore).HasColumnName("rating_score");
        });
    }
}

/// <summary>
/// Read-only projection of a row from the <c>anime_tracking.watch_space_anime</c> table.
/// </summary>
public sealed class AnimeRow
{
    public Guid Id { get; set; }
    public Guid WatchSpaceId { get; set; }
    public string PreferredTitle { get; set; } = string.Empty;
    public string? CoverImageUrlSnapshot { get; set; }
    public int? EpisodeCountSnapshot { get; set; }
    public string? Format { get; set; }
    public string SharedStatus { get; set; } = string.Empty;
    public int SharedEpisodesWatched { get; set; }
    public DateTime AddedAtUtc { get; set; }
    public string? Mood { get; set; }
    public string? Vibe { get; set; }
    public string? Pitch { get; set; }
    public List<ParticipantRow> Participants { get; set; } = [];
}

/// <summary>
/// Read-only projection of a row from the <c>anime_tracking.participant_entries</c> table.
/// </summary>
public sealed class ParticipantRow
{
    public Guid Id { get; set; }
    public Guid WatchSpaceAnimeId { get; set; }
    public Guid UserId { get; set; }
    public decimal? RatingScore { get; set; }
}
