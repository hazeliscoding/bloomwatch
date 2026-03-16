namespace BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;

/// <summary>
/// Represents a query to search for anime titles on AniList.
/// </summary>
/// <param name="Query">The search term used to find matching anime titles.</param>
public sealed record SearchAnimeQuery(string Query);
