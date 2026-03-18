namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.RecordWatchSession;

public sealed record RecordWatchSessionResult(
    Guid Id,
    DateTime SessionDateUtc,
    int StartEpisode,
    int EndEpisode,
    string? Notes,
    Guid CreatedByUserId);
