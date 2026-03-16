using BloomWatch.Modules.AniListSync.Application.Abstractions;

namespace BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;

public sealed class SearchAnimeQueryHandler
{
    private readonly IAniListClient _aniListClient;

    public SearchAnimeQueryHandler(IAniListClient aniListClient)
    {
        _aniListClient = aniListClient;
    }

    public async Task<IReadOnlyList<AnimeSearchResult>> HandleAsync(
        SearchAnimeQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Query))
            throw new ArgumentException("Search query must not be empty.", nameof(query));

        return await _aniListClient.SearchAnimeAsync(query.Query.Trim(), cancellationToken);
    }
}
