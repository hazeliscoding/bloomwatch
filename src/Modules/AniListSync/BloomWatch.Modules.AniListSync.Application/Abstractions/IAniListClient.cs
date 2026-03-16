using BloomWatch.Modules.AniListSync.Application.UseCases.GetMediaDetail;
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

    /// <summary>
    /// Retrieves full metadata for a single AniList anime by its media ID.
    /// </summary>
    /// <param name="anilistMediaId">The AniList media identifier.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="AnimeMediaDetail"/> containing the full metadata, or <c>null</c> if AniList
    /// does not have an entry for the given ID.
    /// </returns>
    Task<AnimeMediaDetail?> GetMediaByIdAsync(int anilistMediaId, CancellationToken cancellationToken = default);
}
