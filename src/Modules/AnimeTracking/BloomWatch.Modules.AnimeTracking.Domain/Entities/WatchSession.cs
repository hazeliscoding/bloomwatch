using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Domain.Entities;

/// <summary>
/// Represents a watch session recording when participants watched episodes together.
/// Owned by the <see cref="Aggregates.WatchSpaceAnime"/> aggregate root.
/// </summary>
public sealed class WatchSession
{
    public Guid Id { get; private set; }
    public WatchSpaceAnimeId WatchSpaceAnimeId { get; private set; }
    public DateTime SessionDateUtc { get; private set; }
    public int StartEpisode { get; private set; }
    public int EndEpisode { get; private set; }
    public string? Notes { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    // Required by EF Core
    private WatchSession() { }

    internal WatchSession(
        WatchSpaceAnimeId watchSpaceAnimeId,
        DateTime sessionDateUtc,
        int startEpisode,
        int endEpisode,
        string? notes,
        Guid createdByUserId)
    {
        Id = Guid.NewGuid();
        WatchSpaceAnimeId = watchSpaceAnimeId;
        SessionDateUtc = sessionDateUtc;
        StartEpisode = startEpisode;
        EndEpisode = endEpisode;
        Notes = notes;
        CreatedByUserId = createdByUserId;
    }
}
