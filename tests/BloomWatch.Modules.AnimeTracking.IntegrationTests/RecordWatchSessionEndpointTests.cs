using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.AnimeTracking.IntegrationTests;

public sealed class RecordWatchSessionEndpointTests : IClassFixture<AnimeTrackingWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly AnimeTrackingWebAppFactory _factory;
    private const int TestMediaId = 154587;

    public RecordWatchSessionEndpointTests(AnimeTrackingWebAppFactory factory)
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

    private async Task<Guid> AddAnimeToSpaceAsync(Guid spaceId, int aniListMediaId = TestMediaId)
    {
        _factory.SeedMediaCache(aniListMediaId);
        var response = await _client.PostAsJsonAsync($"/watchspaces/{spaceId}/anime", new { aniListMediaId });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("watchSpaceAnimeId").GetGuid();
    }

    // --- Tests ---

    [Fact]
    public async Task RecordWatchSession_ValidRequest_Returns201WithSessionDetails()
    {
        var token = await RegisterAndLoginAsync($"session_ok_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);
        var animeId = await AddAnimeToSpaceAsync(spaceId);

        var response = await _client.PostAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{animeId}/sessions",
            new
            {
                sessionDateUtc = "2026-03-15T20:00:00Z",
                startEpisode = 1,
                endEpisode = 3,
                notes = "First session!"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
        body.GetProperty("startEpisode").GetInt32().Should().Be(1);
        body.GetProperty("endEpisode").GetInt32().Should().Be(3);
        body.GetProperty("notes").GetString().Should().Be("First session!");
        body.GetProperty("createdByUserId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task RecordWatchSession_WithoutNotes_Returns201()
    {
        var token = await RegisterAndLoginAsync($"session_nonotes_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);
        var animeId = await AddAnimeToSpaceAsync(spaceId);

        var response = await _client.PostAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{animeId}/sessions",
            new
            {
                sessionDateUtc = "2026-03-15T20:00:00Z",
                startEpisode = 5,
                endEpisode = 5
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RecordWatchSession_StartEpisodeZero_Returns400()
    {
        var token = await RegisterAndLoginAsync($"session_start0_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);
        var animeId = await AddAnimeToSpaceAsync(spaceId);

        var response = await _client.PostAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{animeId}/sessions",
            new
            {
                sessionDateUtc = "2026-03-15T20:00:00Z",
                startEpisode = 0,
                endEpisode = 3
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordWatchSession_EndLessThanStart_Returns400()
    {
        var token = await RegisterAndLoginAsync($"session_endlt_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);
        var animeId = await AddAnimeToSpaceAsync(spaceId);

        var response = await _client.PostAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{animeId}/sessions",
            new
            {
                sessionDateUtc = "2026-03-15T20:00:00Z",
                startEpisode = 5,
                endEpisode = 3
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordWatchSession_NonMember_Returns403()
    {
        var ownerToken = await RegisterAndLoginAsync($"session_owner_{Guid.NewGuid()}@example.com");
        var nonMemberToken = await RegisterAndLoginAsync($"session_nonmember_{Guid.NewGuid()}@example.com");

        var spaceId = await CreateWatchSpaceAsync(ownerToken);
        var animeId = await AddAnimeToSpaceAsync(spaceId);

        WithToken(nonMemberToken);
        var response = await _client.PostAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{animeId}/sessions",
            new
            {
                sessionDateUtc = "2026-03-15T20:00:00Z",
                startEpisode = 1,
                endEpisode = 2
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RecordWatchSession_AnimeNotFound_Returns404()
    {
        var token = await RegisterAndLoginAsync($"session_nf_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);
        var fakeAnimeId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{fakeAnimeId}/sessions",
            new
            {
                sessionDateUtc = "2026-03-15T20:00:00Z",
                startEpisode = 1,
                endEpisode = 1
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordWatchSession_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync(
            $"/watchspaces/{Guid.NewGuid()}/anime/{Guid.NewGuid()}/sessions",
            new
            {
                sessionDateUtc = "2026-03-15T20:00:00Z",
                startEpisode = 1,
                endEpisode = 1
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
