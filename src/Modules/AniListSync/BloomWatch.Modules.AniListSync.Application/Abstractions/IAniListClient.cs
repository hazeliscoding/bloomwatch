using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;

namespace BloomWatch.Modules.AniListSync.Application.Abstractions;

/// <summary>
/// Defines the contract for communicating with the AniList API to retrieve anime data.
/// </summary>
/// <remarks>
/// Implementations may call the AniList GraphQL API directly or apply cross-cutting concerns
/// such as caching via the decorator pattern.
/// </remarks>
public interface IAniListClient
{
    /// <summary>
    /// Searches for anime titles on AniList that match the specified query string.
    /// </summary>
    /// <param name="query">The search term used to find matching anime titles on AniList.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only list of <see cref="AnimeSearchResult"/> objects representing the matching anime.
    /// Returns an empty list when no results are found.
    /// </returns>
    Task<IReadOnlyList<AnimeSearchResult>> SearchAnimeAsync(string query, CancellationToken cancellationToken = default);
}
