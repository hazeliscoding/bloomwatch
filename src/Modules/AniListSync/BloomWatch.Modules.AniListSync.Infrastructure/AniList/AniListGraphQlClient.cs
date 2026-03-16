using System.Net.Http.Json;
using System.Text.Json;
using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;

namespace BloomWatch.Modules.AniListSync.Infrastructure.AniList;

internal sealed class AniListGraphQlClient : IAniListClient
{
    private const string SearchQuery = """
        query ($search: String) {
          Page(page: 1, perPage: 25) {
            media(search: $search, type: ANIME, sort: SEARCH_MATCH) {
              id
              title {
                romaji
                english
              }
              coverImage {
                large
              }
              episodes
              status
              format
              season
              seasonYear
              genres
            }
          }
        }
        """;

    private readonly HttpClient _httpClient;

    public AniListGraphQlClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<AnimeSearchResult>> SearchAnimeAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            query = SearchQuery,
            variables = new { search = query }
        };

        using var response = await _httpClient.PostAsJsonAsync("", requestBody, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            throw new AniListApiException($"AniList API returned HTTP {statusCode}.");
        }

        AniListGraphQlResponse? result;
        try
        {
            result = await response.Content.ReadFromJsonAsync<AniListGraphQlResponse>(cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new AniListApiException("AniList API returned a malformed response.", ex);
        }

        var mediaList = result?.Data?.Page?.Media;
        if (mediaList is null)
            return [];

        return mediaList
            .Select(m => new AnimeSearchResult(
                AnilistMediaId: m.Id,
                TitleRomaji: m.Title?.Romaji,
                TitleEnglish: m.Title?.English,
                CoverImageUrl: m.CoverImage?.Large,
                Episodes: m.Episodes,
                Status: m.Status,
                Format: m.Format,
                Season: m.Season,
                SeasonYear: m.SeasonYear,
                Genres: (IReadOnlyList<string>)(m.Genres ?? [])))
            .ToList();
    }
}

public sealed class AniListApiException : Exception
{
    public AniListApiException(string message) : base(message) { }
    public AniListApiException(string message, Exception innerException) : base(message, innerException) { }
}
