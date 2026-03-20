namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.RecordWatchSession;

/// <summary>
/// Request body for recording a new group watch session for a tracked anime.
/// </summary>
/// <param name="SessionDateUtc">The UTC date and time when the watch session took place.</param>
/// <param name="StartEpisode">The first episode number watched in this session (inclusive).</param>
/// <param name="EndEpisode">The last episode number watched in this session (inclusive). Must be greater than or equal to <paramref name="StartEpisode"/>.</param>
/// <param name="Notes">Optional free-text notes about the session (e.g., reactions, discussion topics).</param>
public sealed record RecordWatchSessionRequest(
    DateTime SessionDateUtc,
    int StartEpisode,
    int EndEpisode,
    string? Notes = null);
