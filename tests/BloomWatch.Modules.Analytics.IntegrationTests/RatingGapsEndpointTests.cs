using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.Analytics.IntegrationTests;

public sealed class RatingGapsEndpointTests : IClassFixture<AnalyticsWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly AnalyticsWebAppFactory _factory;
    private const int TestMediaId1 = 275001;

    public RatingGapsEndpointTests(AnalyticsWebAppFactory factory)
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

    private async Task SubmitRatingAsync(Guid spaceId, Guid animeId, decimal score)
    {
        var response = await _client.PatchAsJsonAsync(
            $"/watchspaces/{spaceId}/anime/{animeId}/participant-rating",
            new { ratingScore = score });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // --- Tests ---

    [Fact]
    public async Task GetRatingGaps_WithData_Returns200WithItems()
    {
        var ownerToken = await RegisterAndLoginAsync($"gaps_data_owner_{Guid.NewGuid()}@example.com", "Owner");

        var spaceId = await CreateWatchSpaceAsync(ownerToken);

        WithToken(ownerToken);
        var animeId = await AddAnimeToSpaceAsync(spaceId, TestMediaId1, "Rated Anime");
        await SubmitRatingAsync(spaceId, animeId, 8.0m);

        // Seed a second rater directly
        _factory.SeedParticipantRating(animeId, Guid.NewGuid(), 5.0m);

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/rating-gaps");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("items").GetArrayLength().Should().Be(1);
        body.GetProperty("items")[0].GetProperty("gap").GetDecimal().Should().Be(3.0m);
        body.GetProperty("items")[0].GetProperty("ratings").GetArrayLength().Should().Be(2);
        body.GetProperty("message").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetRatingGaps_NoQualifyingData_Returns200WithMessage()
    {
        var token = await RegisterAndLoginAsync($"gaps_empty_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/rating-gaps");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("items").GetArrayLength().Should().Be(0);
        body.GetProperty("message").GetString().Should().Be("Not enough data");
    }

    [Fact]
    public async Task GetRatingGaps_NonMember_Returns403()
    {
        var ownerToken = await RegisterAndLoginAsync($"gaps_owner_{Guid.NewGuid()}@example.com");
        var nonMemberToken = await RegisterAndLoginAsync($"gaps_nonmember_{Guid.NewGuid()}@example.com");

        var spaceId = await CreateWatchSpaceAsync(ownerToken);

        WithToken(nonMemberToken);
        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/rating-gaps");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRatingGaps_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/watchspaces/{Guid.NewGuid()}/analytics/rating-gaps");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
