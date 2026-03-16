namespace BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;

/// <summary>
/// Represents a single anime result returned from an AniList search query.
/// </summary>
/// <param name="AnilistMediaId">The unique media identifier assigned by AniList.</param>
/// <param name="TitleRomaji">The romanized (romaji) title of the anime, or <c>null</c> if unavailable.</param>
/// <param name="TitleEnglish">The English-localized title of the anime, or <c>null</c> if unavailable.</param>
/// <param name="CoverImageUrl">The URL of the large cover image, or <c>null</c> if unavailable.</param>
/// <param name="Episodes">The total number of episodes, or <c>null</c> if the count is unknown or the anime is ongoing.</param>
/// <param name="Status">The current airing status (e.g., "FINISHED", "RELEASING"), or <c>null</c> if unavailable.</param>
/// <param name="Format">The media format (e.g., "TV", "MOVIE", "OVA"), or <c>null</c> if unavailable.</param>
/// <param name="Season">The season in which the anime first aired (e.g., "WINTER", "SPRING"), or <c>null</c> if unavailable.</param>
/// <param name="SeasonYear">The year in which the anime's season aired, or <c>null</c> if unavailable.</param>
/// <param name="Genres">A read-only list of genre names associated with the anime.</param>
public sealed record AnimeSearchResult(
    int AnilistMediaId,
    string? TitleRomaji,
    string? TitleEnglish,
    string? CoverImageUrl,
    int? Episodes,
    string? Status,
    string? Format,
    string? Season,
    int? SeasonYear,
    IReadOnlyList<string> Genres);
