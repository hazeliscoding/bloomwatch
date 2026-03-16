namespace BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;

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
