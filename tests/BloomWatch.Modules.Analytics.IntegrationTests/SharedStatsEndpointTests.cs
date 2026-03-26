using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.Analytics.IntegrationTests;

public sealed class SharedStatsEndpointTests : IClassFixture<AnalyticsWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly AnalyticsWebAppFactory _factory;
    private const int TestMediaId1 = 375001;
    private const int TestMediaId2 = 375002;
    private const int TestMediaId3 = 375003;

    public SharedStatsEndpointTests(AnalyticsWebAppFactory factory)
    {
        _factory = factory;
        factory.EnsureSchemaCreated();
        _client = factory.CreateClient();
    }

    // --- Helpers ---

    private async Task<string> RegisterAndLoginAsync(string email, string displayName = "Test User")
    {
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

    private async Task<Guid> CreateWatchSpaceAsync(string token)
    {
        WithToken(token);
        var response = await _client.PostAsJsonAsync("/watchspaces", new { name = $"Space_{Guid.NewGuid():N}" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("watchSpaceId").GetGuid();
    }

    private async Task<Guid> AddAnimeToSpaceAsync(Guid spaceId, int aniListMediaId, string title = "Test Anime")
    {
        _factory.SeedMediaCache(aniListMediaId, title);
        var response = await _client.PostAsJsonAsync($"/watchspaces/{spaceId}/anime", new { aniListMediaId });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("watchSpaceAnimeId").GetGuid();
    }

    private async Task UpdateSharedStatusAsync(Guid spaceId, Guid animeId, string status, int? episodes = null)
    {
        var payload = episodes.HasValue
            ? new { sharedStatus = status, sharedEpisodesWatched = episodes.Value }
            : (object)new { sharedStatus = status };

        var response = await _client.PatchAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{animeId}", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- Tests ---

    [Fact]
    public async Task GetSharedStats_WithData_Returns200WithCorrectStats()
    {
        var token = await RegisterAndLoginAsync($"stats_data_{Guid.NewGuid()}@example.com", "StatsUser");
        var spaceId = await CreateWatchSpaceAsync(token);

        WithToken(token);

        // Add anime with different statuses
        var anime1 = await AddAnimeToSpaceAsync(spaceId, TestMediaId1, "Finished Anime");
        await UpdateSharedStatusAsync(spaceId, anime1, "Finished", 24);

        var anime2 = await AddAnimeToSpaceAsync(spaceId, TestMediaId2, "Dropped Anime");
        await UpdateSharedStatusAsync(spaceId, anime2, "Dropped", 5);

        var anime3 = await AddAnimeToSpaceAsync(spaceId, TestMediaId3, "Watching Anime");
        await UpdateSharedStatusAsync(spaceId, anime3, "Watching", 10);

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/shared-stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("totalEpisodesWatchedTogether").GetInt32().Should().Be(39); // 24+5+10
        body.GetProperty("totalFinished").GetInt32().Should().Be(1);
        body.GetProperty("totalDropped").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetSharedStats_EmptyWatchSpace_Returns200WithZeroesAndNull()
    {
        var token = await RegisterAndLoginAsync($"stats_empty_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/shared-stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("totalEpisodesWatchedTogether").GetInt32().Should().Be(0);
        body.GetProperty("totalFinished").GetInt32().Should().Be(0);
        body.GetProperty("totalDropped").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task GetSharedStats_NonMember_Returns403()
    {
        var ownerToken = await RegisterAndLoginAsync($"stats_owner_{Guid.NewGuid()}@example.com");
        var nonMemberToken = await RegisterAndLoginAsync($"stats_nonmember_{Guid.NewGuid()}@example.com");

        var spaceId = await CreateWatchSpaceAsync(ownerToken);

        WithToken(nonMemberToken);
        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/shared-stats");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSharedStats_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/watchspaces/{Guid.NewGuid()}/analytics/shared-stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
