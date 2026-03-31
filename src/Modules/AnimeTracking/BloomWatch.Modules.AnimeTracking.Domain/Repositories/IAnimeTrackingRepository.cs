using BloomWatch.Modules.AnimeTracking.Domain.Aggregates;
using BloomWatch.Modules.AnimeTracking.Domain.Enums;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Domain.Repositories;

/// <summary>
/// Repository interface for persisting and querying <see cref="WatchSpaceAnime"/> aggregates.
/// </summary>
public interface IAnimeTrackingRepository
{
    /// <summary>
    /// Persists a new <see cref="WatchSpaceAnime"/> aggregate to the store.
    /// </summary>
    /// <param name="anime">The aggregate to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(WatchSpaceAnime anime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an anime with the given AniList media ID already exists in the specified watch space.
    /// </summary>
    /// <param name="watchSpaceId">The watch space to check.</param>
    /// <param name="aniListMediaId">The AniList media ID to look for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the anime is already tracked in the watch space; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(Guid watchSpaceId, int aniListMediaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single <see cref="WatchSpaceAnime"/> aggregate by its composite key,
    /// including all participant entries.
    /// </summary>
    /// <param name="watchSpaceId">The watch space the anime belongs to.</param>
    /// <param name="id">The unique identifier of the tracked anime.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The aggregate if found; otherwise <c>null</c>.</returns>
    Task<WatchSpaceAnime?> GetByIdAsync(Guid watchSpaceId, WatchSpaceAnimeId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all anime tracked in a watch space, optionally filtered by shared status.
    /// Results include participant entries for each anime.
    /// </summary>
    /// <param name="watchSpaceId">The watch space to query.</param>
    /// <param name="statusFilter">An optional status to filter by; <c>null</c> returns all statuses.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<WatchSpaceAnime>> ListByWatchSpaceAsync(Guid watchSpaceId, AnimeStatus? statusFilter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists any pending changes tracked by the underlying context.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
