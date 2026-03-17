namespace BloomWatch.Modules.AnimeTracking.Application.Abstractions;

/// <summary>
/// Looks up cached AniList media metadata. Implemented in Infrastructure
/// by querying the AniListSync module's media_cache table.
/// </summary>
public interface IMediaCacheLookup
{
    Task<MediaCacheSnapshot?> GetByAnilistMediaIdAsync(int anilistMediaId, CancellationToken cancellationToken = default);
}

/// <summary>
/// A read-only snapshot of AniList media metadata used for snapshotting into WatchSpaceAnime.
/// </summary>
public sealed record MediaCacheSnapshot(
    string PreferredTitle,
    int? Episodes,
    string? CoverImageUrl,
    string? Format,
    string? Season,
    int? SeasonYear);
