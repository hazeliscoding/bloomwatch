using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Domain.Repositories;

/// <summary>
/// Repository interface for persisting and querying <see cref="WatchSpaceAnime"/> aggregates.
/// </summary>
public interface IAnimeTrackingRepository
{
    Task AddAsync(WatchSpaceAnime anime, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid watchSpaceId, int aniListMediaId, CancellationToken cancellationToken = default);
    Task<WatchSpaceAnime?> GetByIdAsync(Guid watchSpaceId, WatchSpaceAnimeId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WatchSpaceAnime>> ListByWatchSpaceAsync(Guid watchSpaceId, AnimeStatus? statusFilter, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
