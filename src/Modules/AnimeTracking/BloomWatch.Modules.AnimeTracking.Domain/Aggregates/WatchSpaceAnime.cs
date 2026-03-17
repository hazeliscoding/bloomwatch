using BloomWatch.Modules.AnimeTracking.Domain.Entities;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Domain.Aggregates;

/// <summary>
/// Aggregate root representing an anime being tracked within a watch space.
/// </summary>
public sealed class WatchSpaceAnime
{
    private readonly List<ParticipantEntry> _participantEntries = [];

    public WatchSpaceAnimeId Id { get; private set; }
    public Guid WatchSpaceId { get; private set; }
    public int AniListMediaId { get; private set; }
    public string PreferredTitle { get; private set; } = string.Empty;
    public int? EpisodeCountSnapshot { get; private set; }
    public string? CoverImageUrlSnapshot { get; private set; }
    public string? Format { get; private set; }
    public string? Season { get; private set; }
    public int? SeasonYear { get; private set; }
    public AnimeStatus SharedStatus { get; private set; }
    public int SharedEpisodesWatched { get; private set; }
    public string? Mood { get; private set; }
    public string? Vibe { get; private set; }
    public string? Pitch { get; private set; }
    public Guid AddedByUserId { get; private set; }
    public DateTime AddedAtUtc { get; private set; }

    public IReadOnlyList<ParticipantEntry> ParticipantEntries => _participantEntries.AsReadOnly();

    // Required by EF Core
    private WatchSpaceAnime() { }

    /// <summary>
    /// Creates a new <see cref="WatchSpaceAnime"/> with metadata snapshots and the adding user's
    /// initial <see cref="ParticipantEntry"/>.
    /// </summary>
    public static WatchSpaceAnime Create(
        Guid watchSpaceId,
        int aniListMediaId,
        string preferredTitle,
        int? episodeCountSnapshot,
        string? coverImageUrlSnapshot,
        string? format,
        string? season,
        int? seasonYear,
        string? mood,
        string? vibe,
        string? pitch,
        Guid addedByUserId)
    {
        var now = DateTime.UtcNow;
        var id = WatchSpaceAnimeId.New();

        var anime = new WatchSpaceAnime
        {
            Id = id,
            WatchSpaceId = watchSpaceId,
            AniListMediaId = aniListMediaId,
            PreferredTitle = preferredTitle,
            EpisodeCountSnapshot = episodeCountSnapshot,
            CoverImageUrlSnapshot = coverImageUrlSnapshot,
            Format = format,
            Season = season,
            SeasonYear = seasonYear,
            SharedStatus = AnimeStatus.Backlog,
            SharedEpisodesWatched = 0,
            Mood = mood,
            Vibe = vibe,
            Pitch = pitch,
            AddedByUserId = addedByUserId,
            AddedAtUtc = now
        };

        anime._participantEntries.Add(new ParticipantEntry(id, addedByUserId, now));

        return anime;
    }
}
