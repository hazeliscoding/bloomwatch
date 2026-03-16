using BloomWatch.Modules.AniListSync.Domain.Entities;

namespace BloomWatch.Modules.AniListSync.Application.Abstractions;

/// <summary>
/// Defines the contract for persisting and retrieving cached AniList media entries.
/// </summary>
public interface IMediaCacheRepository
{
    /// <summary>
    /// Retrieves a cached media entry by its AniList media ID.
    /// </summary>
    /// <param name="anilistMediaId">The AniList media identifier.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The cached entry, or <c>null</c> if no entry exists for the given ID.</returns>
    Task<MediaCacheEntry?> GetByAnilistMediaIdAsync(int anilistMediaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a cached media entry using upsert semantics.
    /// </summary>
    /// <param name="entry">The media cache entry to persist.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task UpsertAsync(MediaCacheEntry entry, CancellationToken cancellationToken = default);
}
