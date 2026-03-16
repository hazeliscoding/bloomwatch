using BloomWatch.Modules.AniListSync.Application.Abstractions;

namespace BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;

/// <summary>
/// Handles <see cref="SearchAnimeQuery"/> requests by delegating to the <see cref="IAniListClient"/>.
/// </summary>
public sealed class SearchAnimeQueryHandler
{
    private readonly IAniListClient _aniListClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchAnimeQueryHandler"/> class.
    /// </summary>
    /// <param name="aniListClient">The AniList client used to perform the anime search.</param>
    public SearchAnimeQueryHandler(IAniListClient aniListClient)
    {
        _aniListClient = aniListClient;
    }

    /// <summary>
    /// Executes the search query against the AniList API and returns the matching results.
    /// </summary>
    /// <param name="query">The search query containing the search term.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of <see cref="AnimeSearchResult"/> objects matching the query.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="query"/> contains an empty or whitespace-only search term.</exception>
    public async Task<IReadOnlyList<AnimeSearchResult>> HandleAsync(
        SearchAnimeQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
            throw new ArgumentException("Search query must not be empty.", nameof(query));

        return await _aniListClient.SearchAnimeAsync(query.Query.Trim(), cancellationToken);
    }
}
