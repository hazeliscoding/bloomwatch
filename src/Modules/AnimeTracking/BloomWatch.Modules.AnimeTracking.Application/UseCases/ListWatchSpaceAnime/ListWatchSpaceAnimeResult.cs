namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.ListWatchSpaceAnime;

/// <summary>
/// Result containing the list of anime tracked in a watch space.
/// </summary>
public sealed record ListWatchSpaceAnimeResult(
    IReadOnlyList<WatchSpaceAnimeListItem> Items);

/// <summary>
/// Summary of a single anime entry in a watch space list.
/// </summary>
public sealed record WatchSpaceAnimeListItem(
    Guid WatchSpaceAnimeId,
    int AnilistMediaId,
    string PreferredTitle,
    string? CoverImageUrlSnapshot,
    int? EpisodeCountSnapshot,
    string SharedStatus,
    int SharedEpisodesWatched,
    DateTime AddedAtUtc,
    IReadOnlyList<ParticipantSummary> Participants);

/// <summary>
/// Summary of a single participant's tracking state for an anime.
/// </summary>
public sealed record ParticipantSummary(
    Guid UserId,
    string IndividualStatus,
    int EpisodesWatched);
