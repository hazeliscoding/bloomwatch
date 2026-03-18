namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.GetWatchSpaceAnimeDetail;

/// <summary>
/// Full detail for a single anime tracked in a watch space.
/// </summary>
public sealed record GetWatchSpaceAnimeDetailResult(
    Guid WatchSpaceAnimeId,
    int AnilistMediaId,
    string PreferredTitle,
    string? CoverImageUrlSnapshot,
    int? EpisodeCountSnapshot,
    string? Format,
    string? Season,
    int? SeasonYear,
    string SharedStatus,
    int SharedEpisodesWatched,
    string? Mood,
    string? Vibe,
    string? Pitch,
    Guid AddedByUserId,
    DateTime AddedAtUtc,
    IReadOnlyList<ParticipantDetail> Participants,
    IReadOnlyList<WatchSessionDetail> WatchSessions);

/// <summary>
/// Full participant entry including ratings.
/// </summary>
public sealed record ParticipantDetail(
    Guid UserId,
    string IndividualStatus,
    int EpisodesWatched,
    decimal? RatingScore,
    string? RatingNotes,
    DateTime LastUpdatedAtUtc);

/// <summary>
/// A single watch session record.
/// </summary>
public sealed record WatchSessionDetail(
    Guid WatchSessionId,
    DateTime SessionDateUtc,
    int StartEpisode,
    int EndEpisode,
    string? Notes,
    Guid CreatedByUserId);
