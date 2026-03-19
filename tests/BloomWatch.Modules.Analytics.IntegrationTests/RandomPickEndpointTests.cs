using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.Analytics.IntegrationTests;

public sealed class RandomPickEndpointTests : IClassFixture<AnalyticsWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly AnalyticsWebAppFactory _factory;
    private const int TestMediaId1 = 385001;
    private const int TestMediaId2 = 385002;
    private const int TestMediaId3 = 385003;

    public RandomPickEndpointTests(AnalyticsWebAppFactory factory)
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

    private async Task UpdateSharedStatusAsync(Guid spaceId, Guid animeId, string status)
    {
        var response = await _client.PatchAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{animeId}", new { sharedStatus = status });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- Tests ---

    [Fact]
    public async Task GetRandomPick_BacklogWithAnime_Returns200WithPick()
    {
        var token = await RegisterAndLoginAsync($"pick_data_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);

        WithToken(token);

        // Add anime — default status is Backlog
        var animeId = await AddAnimeToSpaceAsync(spaceId, TestMediaId1, "Backlog Anime");

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/random-pick");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("pick").ValueKind.Should().NotBe(JsonValueKind.Null);
        body.GetProperty("pick").GetProperty("watchSpaceAnimeId").GetGuid().Should().Be(animeId);
        body.GetProperty("pick").GetProperty("preferredTitle").GetString().Should().Be("Backlog Anime");
        body.GetProperty("message").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetRandomPick_EmptyBacklog_Returns200WithNullPickAndMessage()
    {
        var token = await RegisterAndLoginAsync($"pick_empty_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/random-pick");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("pick").ValueKind.Should().Be(JsonValueKind.Null);
        body.GetProperty("message").GetString().Should().Be("Backlog is empty");
    }

    [Fact]
    public async Task GetRandomPick_AllNonBacklog_Returns200WithNullPick()
    {
        var token = await RegisterAndLoginAsync($"pick_nobacklog_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);

        WithToken(token);

        var animeId = await AddAnimeToSpaceAsync(spaceId, TestMediaId2, "Watching Anime");
        await UpdateSharedStatusAsync(spaceId, animeId, "Watching");

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/random-pick");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("pick").ValueKind.Should().Be(JsonValueKind.Null);
        body.GetProperty("message").GetString().Should().Be("Backlog is empty");
    }

    [Fact]
    public async Task GetRandomPick_NonMember_Returns403()
    {
        var ownerToken = await RegisterAndLoginAsync($"pick_owner_{Guid.NewGuid()}@example.com");
        var nonMemberToken = await RegisterAndLoginAsync($"pick_nonmember_{Guid.NewGuid()}@example.com");

        var spaceId = await CreateWatchSpaceAsync(ownerToken);

        WithToken(nonMemberToken);
        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/random-pick");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRandomPick_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/watchspaces/{Guid.NewGuid()}/analytics/random-pick");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
