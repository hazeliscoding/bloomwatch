using BloomWatch.Modules.AnimeTracking.Domain.Entities;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Domain.Aggregates;

/// <summary>
/// Aggregate root representing an anime being tracked within a watch space.
/// <para>
/// Each anime is identified by its AniList media ID and contains metadata snapshots
/// taken at add-time, a shared group tracking state, and individual
/// <see cref="ParticipantEntry"/> records for each member's personal progress and ratings.
/// </para>
/// <para>
/// <b>Invariants:</b>
/// <list type="bullet">
///   <item><description>Episode counts (shared and individual) must be non-negative and cannot exceed the snapshot total.</description></item>
///   <item><description>Ratings must be between 0.5 and 10.0 in 0.5 increments.</description></item>
///   <item><description>Rating notes cannot exceed 1,000 characters.</description></item>
///   <item><description>The adding user automatically receives an initial <see cref="ParticipantEntry"/>.</description></item>
/// </list>
/// </para>
/// </summary>
public sealed class WatchSpaceAnime
{
    private readonly List<ParticipantEntry> _participantEntries = [];

    /// <summary>
    /// Gets the strongly-typed unique identifier for this tracked anime.
    /// </summary>
    public WatchSpaceAnimeId Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the watch space this anime belongs to.
    /// </summary>
    public Guid WatchSpaceId { get; private set; }

    /// <summary>
    /// Gets the AniList media ID used to link this entry to the external AniList catalog.
    /// </summary>
    public int AniListMediaId { get; private set; }

    /// <summary>
    /// Gets the display title (English preferred, falling back to romaji) snapshotted at add-time.
    /// </summary>
    public string PreferredTitle { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the total episode count from the AniList snapshot, or <c>null</c> if unknown or ongoing.
    /// </summary>
    public int? EpisodeCountSnapshot { get; private set; }

    /// <summary>
    /// Gets the cover image URL from the AniList snapshot, or <c>null</c> if unavailable.
    /// </summary>
    public string? CoverImageUrlSnapshot { get; private set; }

    /// <summary>
    /// Gets the media format (e.g., "TV", "MOVIE", "OVA") from the AniList snapshot.
    /// </summary>
    public string? Format { get; private set; }

    /// <summary>
    /// Gets the airing season (e.g., "WINTER", "SPRING") from the AniList snapshot.
    /// </summary>
    public string? Season { get; private set; }

    /// <summary>
    /// Gets the year associated with the airing season from the AniList snapshot.
    /// </summary>
    public int? SeasonYear { get; private set; }

    /// <summary>
    /// Gets the group's collective tracking status for this anime.
    /// Defaults to <see cref="AnimeStatus.Backlog"/> when first added.
    /// </summary>
    public AnimeStatus SharedStatus { get; private set; }

    /// <summary>
    /// Gets the number of episodes the group has collectively watched together.
    /// </summary>
    public int SharedEpisodesWatched { get; private set; }

    /// <summary>
    /// Gets an optional free-text mood tag describing the group's feeling about this pick.
    /// </summary>
    public string? Mood { get; private set; }

    /// <summary>
    /// Gets an optional free-text vibe tag for this anime.
    /// </summary>
    public string? Vibe { get; private set; }

    /// <summary>
    /// Gets an optional short pitch or reason why the group should watch this anime.
    /// </summary>
    public string? Pitch { get; private set; }

    /// <summary>
    /// Gets the identifier of the user who added this anime to the watch space.
    /// </summary>
    public Guid AddedByUserId { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of when this anime was added to the watch space.
    /// </summary>
    public DateTime AddedAtUtc { get; private set; }

    /// <summary>
    /// Gets the individual participant entries (progress, ratings) as a read-only list.
    /// </summary>
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

    /// <summary>
    /// Updates the shared tracking state. Only non-null parameters are applied (partial-patch semantics).
    /// </summary>
    public void UpdateSharedState(
        AnimeStatus? sharedStatus,
        int? sharedEpisodesWatched,
        string? mood,
        string? vibe,
        string? pitch)
    {
        if (sharedEpisodesWatched.HasValue)
        {
            if (sharedEpisodesWatched.Value < 0)
                throw new InvalidSharedStateException("Episodes watched must be non-negative.");

            if (EpisodeCountSnapshot.HasValue && sharedEpisodesWatched.Value > EpisodeCountSnapshot.Value)
                throw new InvalidSharedStateException(
                    $"Episodes watched ({sharedEpisodesWatched.Value}) cannot exceed total episode count ({EpisodeCountSnapshot.Value}).");
        }

        if (sharedStatus.HasValue)
            SharedStatus = sharedStatus.Value;

        if (sharedEpisodesWatched.HasValue)
            SharedEpisodesWatched = sharedEpisodesWatched.Value;

        if (mood is not null)
            Mood = mood;

        if (vibe is not null)
            Vibe = vibe;

        if (pitch is not null)
            Pitch = pitch;
    }

    /// <summary>
    /// Updates (or creates) the requesting user's participant progress.
    /// Returns the updated <see cref="ParticipantEntry"/>.
    /// </summary>
    public ParticipantEntry UpdateParticipantProgress(
        Guid userId,
        AnimeStatus individualStatus,
        int episodesWatched)
    {
        if (episodesWatched < 0)
            throw new InvalidParticipantProgressException("Episodes watched must be non-negative.");

        if (EpisodeCountSnapshot.HasValue && episodesWatched > EpisodeCountSnapshot.Value)
            throw new InvalidParticipantProgressException(
                $"Episodes watched ({episodesWatched}) cannot exceed total episode count ({EpisodeCountSnapshot.Value}).");

        var entry = _participantEntries.FirstOrDefault(e => e.UserId == userId);

        if (entry is not null)
        {
            entry.Update(individualStatus, episodesWatched);
        }
        else
        {
            entry = ParticipantEntry.Create(Id, userId, individualStatus, episodesWatched);
            _participantEntries.Add(entry);
        }

        return entry;
    }

    /// <summary>
    /// Updates (or creates) the requesting user's participant rating.
    /// Returns the updated <see cref="ParticipantEntry"/>.
    /// </summary>
    public ParticipantEntry UpdateParticipantRating(
        Guid userId,
        decimal ratingScore,
        string? ratingNotes,
        bool updateNotes)
    {
        if (ratingScore < 0.5m || ratingScore > 10.0m)
            throw new InvalidRatingException("Rating must be between 0.5 and 10.0.");

        if (ratingScore % 0.5m != 0)
            throw new InvalidRatingException("Rating must be in 0.5 increments.");

        if (updateNotes && ratingNotes is not null && ratingNotes.Length > 1000)
            throw new InvalidRatingException("Rating notes cannot exceed 1000 characters.");

        var entry = _participantEntries.FirstOrDefault(e => e.UserId == userId);

        if (entry is not null)
        {
            entry.UpdateRating(ratingScore, ratingNotes, updateNotes);
        }
        else
        {
            entry = ParticipantEntry.Create(Id, userId, AnimeStatus.Backlog, 0);
            entry.UpdateRating(ratingScore, ratingNotes, updateNotes);
            _participantEntries.Add(entry);
        }

        return entry;
    }
}
