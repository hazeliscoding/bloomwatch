using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;
using Microsoft.Extensions.Caching.Memory;

namespace BloomWatch.Modules.AniListSync.Infrastructure.AniList;

internal sealed class CachedAniListClient : IAniListClient
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IAniListClient _inner;
    private readonly IMemoryCache _cache;

    public CachedAniListClient(IAniListClient inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

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
