using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;
using Microsoft.Extensions.Caching.Memory;

namespace BloomWatch.Modules.AniListSync.Infrastructure.AniList;

/// <summary>
/// A caching decorator around <see cref="IAniListClient"/> that stores search results
/// in an <see cref="IMemoryCache"/> to reduce redundant calls to the AniList API.
/// </summary>
/// <remarks>
/// <para>
/// This class implements the decorator pattern: it wraps an inner <see cref="IAniListClient"/>
/// and checks the in-memory cache before delegating to the inner client. Cache entries
/// are keyed by the normalized (trimmed, lowercased) search query and expire after a
/// 5-minute sliding window.
/// </para>
/// <para>
/// Register this decorator via dependency injection so that all consumers of
/// <see cref="IAniListClient"/> benefit from caching transparently.
/// </para>
/// </remarks>
internal sealed class CachedAniListClient : IAniListClient
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IAniListClient _inner;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedAniListClient"/> class.
    /// </summary>
    /// <param name="inner">The inner <see cref="IAniListClient"/> to delegate to on cache misses.</param>
    /// <param name="cache">The in-memory cache used to store search results.</param>
    public CachedAniListClient(IAniListClient inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    /// <inheritdoc />
    /// <remarks>
    /// On a cache hit the stored results are returned immediately without contacting AniList.
    /// On a cache miss the request is forwarded to the inner client, and the results are
    /// cached for 5 minutes before being returned.
    /// </remarks>
    public async Task<IReadOnlyList<AnimeSearchResult>> SearchAnimeAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"anilist:search:{query.Trim().ToLowerInvariant()}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<AnimeSearchResult>? cached) && cached is not null)
            return cached;

        var results = await _inner.SearchAnimeAsync(query, cancellationToken);

        _cache.Set(cacheKey, results, CacheDuration);

        return results;
    }
}
