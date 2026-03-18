using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.Analytics.IntegrationTests;

public sealed class DashboardSummaryEndpointTests : IClassFixture<AnalyticsWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly AnalyticsWebAppFactory _factory;
    private const int TestMediaId1 = 154587;
    private const int TestMediaId2 = 154588;
    private const int TestMediaId3 = 154589;

    public DashboardSummaryEndpointTests(AnalyticsWebAppFactory factory)
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
        var payload = new Dictionary<string, object?> { ["sharedStatus"] = status };
        if (episodes.HasValue)
            payload["sharedEpisodesWatched"] = episodes.Value;

        var response = await _client.PatchAsJsonAsync($"/watchspaces/{spaceId}/anime/{animeId}", payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task SubmitRatingAsync(Guid spaceId, Guid animeId, decimal score)
    {
        var response = await _client.PatchAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{animeId}/participant-rating",
            new { ratingScore = score });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task InviteAndAcceptAsync(Guid spaceId, string ownerToken, string memberEmail)
    {
        WithToken(ownerToken);
        var inviteResponse = await _client.PostAsJsonAsync($"/watchspaces/{spaceId}/invitations", new { email = memberEmail });
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var inviteBody = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>();
        var invitationId = inviteBody.GetProperty("invitationId").GetGuid();

        // Login as the invited user to accept
        var memberLoginResponse = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = memberEmail,
            password = "password123"
        });
        var memberLoginBody = await memberLoginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var memberToken = memberLoginBody.GetProperty("accessToken").GetString()!;

        WithToken(memberToken);
        var acceptResponse = await _client.PostAsJsonAsync($"/watchspaces/{spaceId}/invitations/{invitationId}/accept", new { });
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- Tests ---

    [Fact]
    public async Task GetDashboard_EmptyWatchSpace_Returns200WithZeroStats()
    {
        var token = await RegisterAndLoginAsync($"dash_empty_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("stats").GetProperty("totalShows").GetInt32().Should().Be(0);
        body.GetProperty("stats").GetProperty("currentlyWatching").GetInt32().Should().Be(0);
        body.GetProperty("stats").GetProperty("finished").GetInt32().Should().Be(0);
        body.GetProperty("stats").GetProperty("episodesWatchedTogether").GetInt32().Should().Be(0);

        body.GetProperty("compatibility").ValueKind.Should().Be(JsonValueKind.Null);
        body.GetProperty("compatibilityMessage").GetString().Should().Be("Not enough data");
        body.GetProperty("currentlyWatching").GetArrayLength().Should().Be(0);
        body.GetProperty("backlogHighlights").GetArrayLength().Should().Be(0);
        body.GetProperty("ratingGapHighlights").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetDashboard_WithData_ReturnsCorrectStats()
    {
        var token = await RegisterAndLoginAsync($"dash_data_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);

        var animeId1 = await AddAnimeToSpaceAsync(spaceId, TestMediaId1, "Anime One");
        var animeId2 = await AddAnimeToSpaceAsync(spaceId, TestMediaId2, "Anime Two");
        var animeId3 = await AddAnimeToSpaceAsync(spaceId, TestMediaId3, "Anime Three");

        // Set statuses
        await UpdateSharedStatusAsync(spaceId, animeId1, "Watching", 5);
        await UpdateSharedStatusAsync(spaceId, animeId2, "Finished", 12);
        // animeId3 remains Backlog

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("stats").GetProperty("totalShows").GetInt32().Should().Be(3);
        body.GetProperty("stats").GetProperty("currentlyWatching").GetInt32().Should().Be(1);
        body.GetProperty("stats").GetProperty("finished").GetInt32().Should().Be(1);
        body.GetProperty("stats").GetProperty("episodesWatchedTogether").GetInt32().Should().Be(17); // 5 + 12 + 0

        body.GetProperty("currentlyWatching").GetArrayLength().Should().Be(1);
        body.GetProperty("backlogHighlights").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task GetDashboard_NonMember_Returns403()
    {
        var ownerToken = await RegisterAndLoginAsync($"dash_owner_{Guid.NewGuid()}@example.com");
        var nonMemberToken = await RegisterAndLoginAsync($"dash_nonmember_{Guid.NewGuid()}@example.com");

        var spaceId = await CreateWatchSpaceAsync(ownerToken);

        WithToken(nonMemberToken);
        var response = await _client.GetAsync($"/watchspaces/{spaceId}/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDashboard_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/watchspaces/{Guid.NewGuid()}/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDashboard_NonExistentWatchSpace_Returns403()
    {
        // A non-existent watch space returns 403 because membership check fails first
        // (per spec: membership is checked before existence to prevent information leakage)
        var token = await RegisterAndLoginAsync($"dash_notfound_{Guid.NewGuid()}@example.com");
        WithToken(token);

        var response = await _client.GetAsync($"/watchspaces/{Guid.NewGuid()}/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
