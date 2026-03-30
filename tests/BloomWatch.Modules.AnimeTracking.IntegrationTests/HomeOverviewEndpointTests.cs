using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.AnimeTracking.IntegrationTests;

public sealed class HomeOverviewEndpointTests : IClassFixture<AnimeTrackingWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly AnimeTrackingWebAppFactory _factory;

    public HomeOverviewEndpointTests(AnimeTrackingWebAppFactory factory)
    {
        _factory = factory;
        factory.EnsureSchemaCreated();
        _client = factory.CreateClient();
    }

    // --- Helpers ---

    private async Task<string> RegisterAndLoginAsync(string? email = null, string displayName = "Test User")
    {
        email ??= $"home_{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "password123",
            displayName
        });

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "password123"
        });

        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private void WithToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> CreateWatchSpaceAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/watchspaces", new { name });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("watchSpaceId").GetGuid().ToString();
    }

    private async Task AddAnimeAsync(string watchSpaceId, int aniListMediaId)
    {
        _factory.SeedMediaCache(aniListMediaId, $"Anime {aniListMediaId}");
        var response = await _client.PostAsJsonAsync($"/watchspaces/{watchSpaceId}/anime", new
        {
            aniListMediaId
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // --- Tests ---

    [Fact]
    public async Task GetHomeOverview_Unauthenticated_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/home/overview");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHomeOverview_NoWatchSpaces_ReturnsEmptyOverview()
    {
        var token = await RegisterAndLoginAsync(displayName: "Lonely User");
        WithToken(token);

        var response = await _client.GetAsync("/home/overview");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("displayName").GetString().Should().Be("Lonely User");
        body.GetProperty("stats").GetProperty("watchSpaceCount").GetInt32().Should().Be(0);
        body.GetProperty("stats").GetProperty("totalAnimeTracked").GetInt32().Should().Be(0);
        body.GetProperty("stats").GetProperty("totalEpisodesWatchedTogether").GetInt32().Should().Be(0);
        body.GetProperty("watchSpaces").GetArrayLength().Should().Be(0);
        body.GetProperty("recentActivity").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetHomeOverview_WithWatchSpaceAndAnime_ReturnsOverview()
    {
        var token = await RegisterAndLoginAsync(displayName: "Active User");
        WithToken(token);

        var spaceId = await CreateWatchSpaceAsync("Test Space");
        await AddAnimeAsync(spaceId, 101);
        await AddAnimeAsync(spaceId, 102);

        var response = await _client.GetAsync("/home/overview");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("displayName").GetString().Should().Be("Active User");
        body.GetProperty("stats").GetProperty("watchSpaceCount").GetInt32().Should().Be(1);
        body.GetProperty("stats").GetProperty("totalAnimeTracked").GetInt32().Should().Be(2);
        body.GetProperty("watchSpaces").GetArrayLength().Should().Be(1);

        var space = body.GetProperty("watchSpaces")[0];
        space.GetProperty("name").GetString().Should().Be("Test Space");
        space.GetProperty("backlogCount").GetInt32().Should().Be(2); // new anime default to Backlog
    }
}
