using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BloomWatch.Modules.Identity.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<IdentityWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(IdentityWebAppFactory factory)
    {
        factory.EnsureSchemaCreated();
        _client = factory.CreateClient();
    }

    // --- POST /auth/register ---

    [Fact]
    public async Task Register_ValidRequest_Returns201WithUserProfile()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            email = $"valid_{Guid.NewGuid()}@example.com",
            password = "password123",
            displayName = "New User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("userId").GetGuid().Should().NotBeEmpty();
        body.GetProperty("displayName").GetString().Should().Be("New User");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = $"dup_{Guid.NewGuid()}@example.com";
        var payload = new { email, password = "password123", displayName = "First" };

        var first = await _client.PostAsJsonAsync("/auth/register", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/auth/register", payload);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- POST /auth/login ---

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var email = $"login_{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "password123",
            displayName = "Login Test"
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("expiresAt").GetDateTime().Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_BadPassword_Returns401()
    {
        var email = $"badpw_{Guid.NewGuid()}@example.com";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "password123",
            displayName = "Bad PW Test"
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "wrongpassword"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- JWT middleware ---

    [Fact]
    public async Task ProtectedRoute_NoToken_Returns401()
    {
        // Placeholder - requires a real [Authorize] endpoint once the first
        // protected feature is implemented.
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ProtectedRoute_ValidToken_Returns200()
    {
        // Placeholder - requires a real [Authorize] endpoint.
        await Task.CompletedTask;
    }
}
