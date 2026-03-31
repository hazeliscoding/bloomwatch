using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Domain.Entities;

/// <summary>
/// Represents a single user's individual tracking state for an anime within a watch space.
/// Owned by the <see cref="Aggregates.WatchSpaceAnime"/> aggregate root.
/// </summary>
public sealed class ParticipantEntry
{
    /// <summary>
    /// Gets the unique identifier for this participant entry.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the parent <see cref="Aggregates.WatchSpaceAnime"/> aggregate.
    /// </summary>
    public WatchSpaceAnimeId WatchSpaceAnimeId { get; private set; }

    /// <summary>
    /// Gets the identifier of the user this entry belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the user's personal tracking status for this anime.
    /// </summary>
    public AnimeStatus IndividualStatus { get; private set; }

    /// <summary>
    /// Gets the number of episodes this user has individually watched.
    /// </summary>
    public int EpisodesWatched { get; private set; }

    /// <summary>
    /// Gets the user's numeric rating (0.5–10.0 in 0.5 increments), or <c>null</c> if not yet rated.
    /// </summary>
    public decimal? RatingScore { get; private set; }

    /// <summary>
    /// Gets the user's free-text notes accompanying the rating, or <c>null</c> if none provided.
    /// </summary>
    public string? RatingNotes { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of the last update to this entry (progress or rating).
    /// </summary>
    public DateTime LastUpdatedAtUtc { get; private set; }

    // Required by EF Core
    private ParticipantEntry() { }

    /// <summary>
    /// Initializes a new participant entry with <see cref="AnimeStatus.Backlog"/> status
    /// and zero episodes watched. Used when the aggregate creates the initial entry for the adding user.
    /// </summary>
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

    /// <summary>
    /// Creates a new participant entry with the specified status and episode count.
    /// Used when a user first updates their progress on an anime they haven't tracked yet.
    /// </summary>
    internal static ParticipantEntry Create(
        WatchSpaceAnimeId watchSpaceAnimeId,
        Guid userId,
        AnimeStatus individualStatus,
        int episodesWatched)
    {
        return new ParticipantEntry
        {
            Id = Guid.NewGuid(),
            WatchSpaceAnimeId = watchSpaceAnimeId,
            UserId = userId,
            IndividualStatus = individualStatus,
            EpisodesWatched = episodesWatched,
            LastUpdatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the participant's individual tracking status and episode count.
    /// </summary>
    /// <param name="individualStatus">The new tracking status.</param>
    /// <param name="episodesWatched">The updated episode count.</param>
    internal void Update(AnimeStatus individualStatus, int episodesWatched)
    {
        IndividualStatus = individualStatus;
        EpisodesWatched = episodesWatched;
        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the participant's rating score and optionally their rating notes.
    /// </summary>
    /// <param name="ratingScore">The new rating score (0.5–10.0).</param>
    /// <param name="ratingNotes">The optional notes, or <c>null</c> to clear.</param>
    /// <param name="updateNotes">
    /// If <c>true</c>, the <see cref="RatingNotes"/> field is updated to <paramref name="ratingNotes"/>;
    /// if <c>false</c>, existing notes are left unchanged.
    /// </param>
    internal void UpdateRating(decimal ratingScore, string? ratingNotes, bool updateNotes)
    {
        RatingScore = ratingScore;
        if (updateNotes)
            RatingNotes = ratingNotes;
        LastUpdatedAtUtc = DateTime.UtcNow;
    }
}
