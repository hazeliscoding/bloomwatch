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
    IReadOnlyList<string>? Genres = null,
    string? Description = null,
    int? AverageScore = null,
    int? Popularity = null,
    IReadOnlyList<AnimeTagDetail>? Tags = null,
    string? SiteUrl = null,
    string? AiringStatus = null);

/// <summary>
/// Tag detail included in the anime detail response.
/// </summary>
public sealed record AnimeTagDetail(string Name, int Rank, bool IsMediaSpoiler);

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
