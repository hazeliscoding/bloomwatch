using BloomWatch.Modules.Analytics.Application.DTOs;

namespace BloomWatch.Modules.Analytics.Application.Abstractions;

/// <summary>
/// Cross-module read interface for querying AnimeTracking data.
/// Implemented in Infrastructure with direct database access to the anime_tracking schema.
/// </summary>
public interface IWatchSpaceAnalyticsDataSource
{
    /// <summary>
    /// Loads all anime in a watch space with their participant entries (including ratings).
    /// </summary>
    Task<IReadOnlyList<WatchSpaceAnimeData>> GetAnimeWithParticipantsAsync(
        Guid watchSpaceId, CancellationToken cancellationToken = default);
}
