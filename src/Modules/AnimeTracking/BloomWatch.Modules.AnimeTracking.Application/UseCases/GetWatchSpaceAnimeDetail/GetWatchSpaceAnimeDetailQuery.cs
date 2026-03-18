namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.GetWatchSpaceAnimeDetail;

/// <summary>
/// Query to retrieve full detail for a single anime in a watch space.
/// </summary>
public sealed record GetWatchSpaceAnimeDetailQuery(
    Guid WatchSpaceId,
    Guid WatchSpaceAnimeId,
    Guid RequestingUserId);
