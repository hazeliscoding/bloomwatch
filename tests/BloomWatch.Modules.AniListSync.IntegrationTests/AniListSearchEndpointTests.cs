using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.AniListSync.IntegrationTests;

public sealed class AniListSearchEndpointTests : IClassFixture<AniListSyncWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly AniListSyncWebAppFactory _factory;

    private const string SampleAniListResponse = """
        {
          "data": {
            "Page": {
              "media": [
                {
                  "id": 1,
                  "title": { "romaji": "Cowboy Bebop", "english": "Cowboy Bebop" },
                  "coverImage": { "large": "https://img.example.com/1.jpg" },
                  "episodes": 26,
                  "status": "FINISHED",
                  "format": "TV",
                  "season": "SPRING",
                  "seasonYear": 1998,
                  "genres": ["Action", "Adventure", "Sci-Fi"]
                }
              ]
            }
          }
        }
        """;

    private const string EmptyAniListResponse = """
        {
          "data": {
            "Page": {
              "media": []
            }
          }
        }
        """;

    public AniListSearchEndpointTests(AniListSyncWebAppFactory factory)
    {
        _factory = factory;
        factory.EnsureSchemaCreated();
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginAsync()
    {
        var email = $"anilist_{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "password123",
            displayName = "Test User"
        });

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "password123"
        });

        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task Search_Authenticated_WithResults_Returns200WithAnimeData()
    {
        _factory.StubHandler.SetupSuccess(SampleAniListResponse);
        var token = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/anilist/search?query=Cowboy%20Bebop");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<JsonElement>();
        results.GetArrayLength().Should().Be(1);

        var first = results[0];
        first.GetProperty("anilistMediaId").GetInt32().Should().Be(1);
        first.GetProperty("titleRomaji").GetString().Should().Be("Cowboy Bebop");
        first.GetProperty("titleEnglish").GetString().Should().Be("Cowboy Bebop");
        first.GetProperty("coverImageUrl").GetString().Should().Be("https://img.example.com/1.jpg");
        first.GetProperty("episodes").GetInt32().Should().Be(26);
        first.GetProperty("status").GetString().Should().Be("FINISHED");
        first.GetProperty("format").GetString().Should().Be("TV");
        first.GetProperty("season").GetString().Should().Be("SPRING");
        first.GetProperty("seasonYear").GetInt32().Should().Be(1998);
        first.GetProperty("genres").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task Search_Authenticated_NoResults_Returns200WithEmptyArray()
    {
        _factory.StubHandler.SetupSuccess(EmptyAniListResponse);
        var token = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/anilist/search?query=xyznonexistent999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<JsonElement>();
        results.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Search_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/anilist/search?query=Naruto");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Search_MissingQuery_Returns400()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/anilist/search");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_EmptyQuery_Returns400()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/anilist/search?query=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WhitespaceQuery_Returns400()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/anilist/search?query=%20%20");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_AniListReturnsError_Returns502()
    {
        _factory.StubHandler.SetupError(HttpStatusCode.InternalServerError);
        var token = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/anilist/search?query=test");

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }
}
