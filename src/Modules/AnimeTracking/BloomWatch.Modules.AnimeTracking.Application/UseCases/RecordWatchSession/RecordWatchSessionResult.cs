namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.RecordWatchSession;

/// <summary>
/// Result returned after successfully recording a watch session.
/// </summary>
/// <param name="Id">The unique identifier of the newly created watch session.</param>
/// <param name="SessionDateUtc">The UTC date and time of the session.</param>
/// <param name="StartEpisode">The first episode watched (inclusive).</param>
/// <param name="EndEpisode">The last episode watched (inclusive).</param>
/// <param name="Notes">Free-text notes about the session, or <c>null</c> if none were provided.</param>
/// <param name="CreatedByUserId">The unique identifier of the user who recorded this session.</param>
public sealed record RecordWatchSessionResult(
    Guid Id,
    DateTime SessionDateUtc,
    int StartEpisode,
    int EndEpisode,
    string? Notes,
    Guid CreatedByUserId);
