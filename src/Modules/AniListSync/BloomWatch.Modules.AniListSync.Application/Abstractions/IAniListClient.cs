using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;

namespace BloomWatch.Modules.AniListSync.Application.Abstractions;

public interface IAniListClient
{
    Task<IReadOnlyList<AnimeSearchResult>> SearchAnimeAsync(string query, CancellationToken cancellationToken = default);
}
