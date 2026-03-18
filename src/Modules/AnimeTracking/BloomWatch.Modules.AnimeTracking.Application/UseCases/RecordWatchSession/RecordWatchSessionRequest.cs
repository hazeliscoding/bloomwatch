namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.RecordWatchSession;

public sealed record RecordWatchSessionRequest(
    DateTime SessionDateUtc,
    int StartEpisode,
    int EndEpisode,
    string? Notes = null);
