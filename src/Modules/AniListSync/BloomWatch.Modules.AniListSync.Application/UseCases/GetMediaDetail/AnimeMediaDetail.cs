using BloomWatch.Modules.AniListSync.Domain.Entities;

namespace BloomWatch.Modules.AniListSync.Application.UseCases.GetMediaDetail;

/// <summary>
/// Represents the full cached metadata for a single AniList anime entry.
/// </summary>
/// <remarks>
/// This is a superset of <see cref="SearchAnime.AnimeSearchResult"/>, adding
/// <c>TitleNative</c>, <c>Description</c>, <c>AverageScore</c>, <c>Popularity</c>,
/// <c>Tags</c>, <c>SiteUrl</c>, and the <c>CachedAt</c> timestamp indicating data freshness.
/// </remarks>
public sealed record AnimeMediaDetail(
    int AnilistMediaId,
    string? TitleRomaji,
    string? TitleEnglish,
    string? TitleNative,
    string? CoverImageUrl,
    int? Episodes,
    string? Status,
    string? Format,
    string? Season,
    int? SeasonYear,
    IReadOnlyList<string> Genres,
    string? Description,
    int? AverageScore,
    int? Popularity,
    IReadOnlyList<MediaTag> Tags,
    string? SiteUrl,
    DateTime CachedAt);
