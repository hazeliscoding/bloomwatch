using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.Identity.IntegrationTests;

public sealed class RefreshTokenEndpointTests : IClassFixture<IdentityWebAppFactory>
{
    private readonly HttpClient _client;

    public RefreshTokenEndpointTests(IdentityWebAppFactory factory)
    {
        factory.EnsureSchemaCreated();
        _client = factory.CreateClient();
    }

    private async Task<(string AccessToken, string RefreshToken)> RegisterAndLoginAsync()
    {
        var email = $"rt_{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "password123",
            displayName = "RT Test"
        });

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "password123"
        });

        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = body.GetProperty("accessToken").GetString()!;
        var refreshToken = body.GetProperty("refreshToken").GetString()!;
        return (accessToken, refreshToken);
    }

    // --- POST /auth/refresh ---

    [Fact]
    public async Task Refresh_ValidToken_Returns200WithNewTokenPair()
    {
        var (_, refreshToken) = await RegisterAndLoginAsync();

        var response = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("expiresAt").GetDateTime().Should().BeAfter(DateTime.UtcNow);
        body.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("refreshTokenExpiresAt").GetDateTime().Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Refresh_TokenIsRotated_OldTokenNoLongerValid()
    {
        var (_, refreshToken) = await RegisterAndLoginAsync();

        // Use it once — succeeds
        await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken });

        // Use the same token again — should fail
        var secondResponse = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken });
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_InvalidToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken = "not-a-real-token" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- POST /auth/revoke ---

    [Fact]
    public async Task Revoke_ValidToken_Returns204()
    {
        var (_, refreshToken) = await RegisterAndLoginAsync();

        var response = await _client.PostAsJsonAsync("/auth/revoke", new { refreshToken });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Revoke_RevokedToken_CannotBeUsedToRefresh()
    {
        var (_, refreshToken) = await RegisterAndLoginAsync();

        await _client.PostAsJsonAsync("/auth/revoke", new { refreshToken });

        var refreshResponse = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken });
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Revoke_UnknownToken_Returns204()
    {
        var response = await _client.PostAsJsonAsync("/auth/revoke", new { refreshToken = "unknown-token" });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
