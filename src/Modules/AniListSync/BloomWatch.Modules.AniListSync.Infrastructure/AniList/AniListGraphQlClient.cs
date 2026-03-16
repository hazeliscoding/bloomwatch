using System.Net.Http.Json;
using System.Text.Json;
using BloomWatch.Modules.AniListSync.Application.Abstractions;
using BloomWatch.Modules.AniListSync.Application.UseCases.SearchAnime;

namespace BloomWatch.Modules.AniListSync.Infrastructure.AniList;

/// <summary>
/// Communicates with the AniList GraphQL API over HTTP to search for anime media.
/// </summary>
/// <remarks>
/// This client sends a paginated GraphQL query to the AniList <c>Page</c> endpoint,
/// requesting up to 25 results sorted by search relevance. The raw GraphQL response
/// is deserialized into <see cref="AniListGraphQlResponse"/> and mapped to
/// <see cref="AnimeSearchResult"/> domain objects.
/// </remarks>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AniListGraphQlClient"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The <see cref="HttpClient"/> configured with the AniList GraphQL base address.
    /// </param>
    public AniListGraphQlClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    /// <exception cref="AniListApiException">
    /// Thrown when the AniList API returns a non-success HTTP status code or a malformed JSON response.
    /// </exception>
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

/// <summary>
/// Represents an error that occurs during communication with the AniList API.
/// </summary>
/// <remarks>
/// This exception is thrown when the AniList GraphQL endpoint returns a non-success HTTP
/// status code or when the response body cannot be deserialized as valid JSON.
/// </remarks>
public sealed class AniListApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AniListApiException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">A message describing the API error.</param>
    public AniListApiException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AniListApiException"/> class with a specified error message
    /// and a reference to the inner exception that caused this error.
    /// </summary>
    /// <param name="message">A message describing the API error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public AniListApiException(string message, Exception innerException) : base(message, innerException) { }
}
