using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.AniListSync.IntegrationTests;

public sealed class AniListMediaDetailEndpointTests : IClassFixture<AniListSyncWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly AniListSyncWebAppFactory _factory;

    private const string SampleMediaDetailResponse = """
        {
          "data": {
            "Media": {
              "id": 1,
              "title": { "romaji": "Cowboy Bebop", "english": "Cowboy Bebop", "native": "カウボーイビバップ" },
              "coverImage": { "large": "https://img.example.com/1.jpg" },
              "episodes": 26,
              "status": "FINISHED",
              "format": "TV",
              "season": "SPRING",
              "seasonYear": 1998,
              "genres": ["Action", "Adventure", "Sci-Fi"],
              "description": "A bounty hunter crew in space.",
              "averageScore": 86,
              "popularity": 200000
            }
          }
        }
        """;

    private const string NullMediaResponse = """
        {
          "data": {
            "Media": null
          }
        }
        """;

    public AniListMediaDetailEndpointTests(AniListSyncWebAppFactory factory)
    {
        _factory = factory;
        factory.EnsureSchemaCreated();
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginAsync()
    {
        var email = $"detail_{Guid.NewGuid()}@example.com";

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
    public async Task GetMediaDetail_Authenticated_ValidId_Returns200WithFullMetadata()
    {
        _factory.StubHandler.SetupSuccess(SampleMediaDetailResponse);
        var token = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/anilist/media/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("anilistMediaId").GetInt32().Should().Be(1);
        result.GetProperty("titleRomaji").GetString().Should().Be("Cowboy Bebop");
        result.GetProperty("titleEnglish").GetString().Should().Be("Cowboy Bebop");
        result.GetProperty("titleNative").GetString().Should().Be("カウボーイビバップ");
        result.GetProperty("description").GetString().Should().Be("A bounty hunter crew in space.");
        result.GetProperty("averageScore").GetInt32().Should().Be(86);
        result.GetProperty("popularity").GetInt32().Should().Be(200000);
        result.GetProperty("cachedAt").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetMediaDetail_Authenticated_UnknownId_Returns404()
    {
        _factory.StubHandler.SetupSuccess(NullMediaResponse);
        var token = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/anilist/media/999999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMediaDetail_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/anilist/media/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMediaDetail_AniListError_Returns502()
    {
        _factory.StubHandler.SetupError(HttpStatusCode.InternalServerError);
        var token = await RegisterAndLoginAsync();

        // Use a media ID that won't be cached from other tests
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/anilist/media/77777");

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }
}
