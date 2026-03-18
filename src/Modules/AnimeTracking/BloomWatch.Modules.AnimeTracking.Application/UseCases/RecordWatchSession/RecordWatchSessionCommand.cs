namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.RecordWatchSession;

public sealed record RecordWatchSessionCommand(
    Guid WatchSpaceId,
    Guid WatchSpaceAnimeId,
    Guid RequestingUserId,
    DateTime SessionDateUtc,
    int StartEpisode,
    int EndEpisode,
    string? Notes);
