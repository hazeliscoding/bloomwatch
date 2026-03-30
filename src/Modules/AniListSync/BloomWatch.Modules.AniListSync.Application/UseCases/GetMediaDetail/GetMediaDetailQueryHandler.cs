using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Domain.Entities;

namespace BloomWatch.Modules.AniListSync.Application.UseCases.GetMediaDetail;

/// <summary>
/// Handles <see cref="GetMediaDetailQuery"/> requests by checking the persistent cache first,
/// then falling back to the AniList API on cache miss or stale entry.
/// </summary>
public sealed class GetMediaDetailQueryHandler
{
    private static readonly TimeSpan CacheFreshness = TimeSpan.FromHours(24);

    private readonly IMediaCacheRepository _cacheRepository;
    private readonly IAniListClient _aniListClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMediaDetailQueryHandler"/> class.
    /// </summary>
    public GetMediaDetailQueryHandler(IMediaCacheRepository cacheRepository, IAniListClient aniListClient)
    {
        _cacheRepository = cacheRepository;
        _aniListClient = aniListClient;
    }

    /// <summary>
    /// Executes the media detail query. Returns cached data if fresh, otherwise fetches from AniList.
    /// </summary>
    /// <returns>
    /// An <see cref="AnimeMediaDetail"/> if the media exists, or <c>null</c> if AniList does not
    /// know the requested ID.
    /// </returns>
    /// <exception cref="AniListApiException">
    /// Re-thrown when the AniList API fails and no cached data is available to serve.
    /// </exception>
    public async Task<AnimeMediaDetail?> HandleAsync(
        GetMediaDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        var cached = await _cacheRepository.GetByAnilistMediaIdAsync(query.AnilistMediaId, cancellationToken);

        if (cached is not null && IsFresh(cached.CachedAt))
            return ToDetail(cached);

        AnimeMediaDetail? fetched;
        try
        {
            fetched = await _aniListClient.GetMediaByIdAsync(query.AnilistMediaId, cancellationToken);
        }
        catch when (cached is not null)
        {
            // AniList failure with stale cache — return 502, preserve cache entry
            throw;
        }

        if (fetched is null)
            return null;

        var now = DateTime.UtcNow;
        if (cached is null)
        {
            var entry = MediaCacheEntry.Create(
                fetched.AnilistMediaId,
                fetched.TitleRomaji,
                fetched.TitleEnglish,
                fetched.TitleNative,
                fetched.CoverImageUrl,
                fetched.Episodes,
                fetched.Status,
                fetched.Format,
                fetched.Season,
                fetched.SeasonYear,
                fetched.Genres,
                fetched.Description,
                fetched.AverageScore,
                fetched.Popularity,
                fetched.Tags,
                fetched.SiteUrl,
                now);
            await _cacheRepository.UpsertAsync(entry, cancellationToken);
        }
        else
        {
            cached.Update(
                fetched.TitleRomaji,
                fetched.TitleEnglish,
                fetched.TitleNative,
                fetched.CoverImageUrl,
                fetched.Episodes,
                fetched.Status,
                fetched.Format,
                fetched.Season,
                fetched.SeasonYear,
                fetched.Genres,
                fetched.Description,
                fetched.AverageScore,
                fetched.Popularity,
                fetched.Tags,
                fetched.SiteUrl,
                now);
            await _cacheRepository.UpsertAsync(cached, cancellationToken);
        }

        return fetched with { CachedAt = now };
    }

    private static bool IsFresh(DateTime cachedAt) =>
        DateTime.UtcNow - cachedAt < CacheFreshness;

    private static AnimeMediaDetail ToDetail(MediaCacheEntry entry) =>
        new(
            entry.AnilistMediaId,
            entry.TitleRomaji,
            entry.TitleEnglish,
            entry.TitleNative,
            entry.CoverImageUrl,
            entry.Episodes,
            entry.Status,
            entry.Format,
            entry.Season,
            entry.SeasonYear,
            entry.Genres,
            entry.Description,
            entry.AverageScore,
            entry.Popularity,
            entry.Tags,
            entry.SiteUrl,
            entry.CachedAt);
}
