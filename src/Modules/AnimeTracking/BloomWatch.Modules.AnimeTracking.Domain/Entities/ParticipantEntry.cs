using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Domain.Entities;

/// <summary>
/// Represents a single user's individual tracking state for an anime within a watch space.
/// Owned by the <see cref="Aggregates.WatchSpaceAnime"/> aggregate root.
/// </summary>
public sealed class ParticipantEntry
{
    public Guid Id { get; private set; }
    public WatchSpaceAnimeId WatchSpaceAnimeId { get; private set; }
    public Guid UserId { get; private set; }
    public AnimeStatus IndividualStatus { get; private set; }
    public int EpisodesWatched { get; private set; }
    public decimal? RatingScore { get; private set; }
    public string? RatingNotes { get; private set; }
    public DateTime LastUpdatedAtUtc { get; private set; }

    // Required by EF Core
    private ParticipantEntry() { }

    internal ParticipantEntry(
        WatchSpaceAnimeId watchSpaceAnimeId,
        Guid userId,
        DateTime now)
    {
        Id = Guid.NewGuid();
        WatchSpaceAnimeId = watchSpaceAnimeId;
        UserId = userId;
        IndividualStatus = AnimeStatus.Backlog;
        EpisodesWatched = 0;
        LastUpdatedAtUtc = now;
    }
}
