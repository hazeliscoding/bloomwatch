using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.Analytics.IntegrationTests;

public sealed class CompatibilityEndpointTests : IClassFixture<AnalyticsWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly AnalyticsWebAppFactory _factory;
    private const int TestMediaId1 = 265001;

    public CompatibilityEndpointTests(AnalyticsWebAppFactory factory)
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
        var responseBody = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Rating submission failed: {responseBody}");
    }

    // --- Tests ---

    [Fact]
    public async Task GetCompatibility_WithRatingData_Returns200WithScore()
    {
        var ownerToken = await RegisterAndLoginAsync($"compat_data_owner_{Guid.NewGuid()}@example.com", "Owner");

        var spaceId = await CreateWatchSpaceAsync(ownerToken);

        // Add anime — the owner automatically gets a participant entry
        WithToken(ownerToken);
        var animeId = await AddAnimeToSpaceAsync(spaceId, TestMediaId1, "Rated Anime");

        // Owner rates via HTTP
        await SubmitRatingAsync(spaceId, animeId, 8.0m);

        // Seed a second participant's rating directly (avoids SQLite concurrency issue with EF Core upsert)
        _factory.SeedParticipantRating(animeId, Guid.NewGuid(), 6.0m);

        // Fetch compatibility
        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/compatibility");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("compatibility").ValueKind.Should().Be(JsonValueKind.Object);
        body.GetProperty("compatibility").GetProperty("score").GetInt32().Should().BeInRange(0, 100);
        body.GetProperty("compatibility").GetProperty("averageGap").GetDecimal().Should().Be(2.0m);
        body.GetProperty("compatibility").GetProperty("ratedTogetherCount").GetInt32().Should().Be(1);
        body.GetProperty("compatibility").GetProperty("label").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("message").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetCompatibility_InsufficientData_Returns200WithNull()
    {
        var token = await RegisterAndLoginAsync($"compat_empty_{Guid.NewGuid()}@example.com");
        var spaceId = await CreateWatchSpaceAsync(token);

        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/compatibility");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("compatibility").ValueKind.Should().Be(JsonValueKind.Null);
        body.GetProperty("message").GetString().Should().Be("Not enough data");
    }

    [Fact]
    public async Task GetCompatibility_NonMember_Returns403()
    {
        var ownerToken = await RegisterAndLoginAsync($"compat_owner_{Guid.NewGuid()}@example.com");
        var nonMemberToken = await RegisterAndLoginAsync($"compat_nonmember_{Guid.NewGuid()}@example.com");

        var spaceId = await CreateWatchSpaceAsync(ownerToken);

        WithToken(nonMemberToken);
        var response = await _client.GetAsync($"/watchspaces/{spaceId}/analytics/compatibility");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCompatibility_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/watchspaces/{Guid.NewGuid()}/analytics/compatibility");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
