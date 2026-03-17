using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;

namespace BloomWatch.Modules.AnimeTracking.Domain.Repositories;

/// <summary>
/// Repository interface for persisting and querying <see cref="WatchSpaceAnime"/> aggregates.
/// </summary>
public interface IAnimeTrackingRepository
{
    Task AddAsync(WatchSpaceAnime anime, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid watchSpaceId, int aniListMediaId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
