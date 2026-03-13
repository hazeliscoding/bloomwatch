using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace BloomWatch.Modules.Identity.IntegrationTests;

public sealed class UserProfileEndpointTests : IClassFixture<IdentityWebAppFactory>
{
    private readonly HttpClient _client;

    public UserProfileEndpointTests(IdentityWebAppFactory factory)
    {
        factory.EnsureSchemaCreated();
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndLoginAsync(string? email = null, string displayName = "Test User")
    {
        email ??= $"profile_{Guid.NewGuid()}@example.com";

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

    private static string GenerateTokenForNonexistentUser()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("CHANGE-ME-use-a-long-random-secret-in-production"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "BloomWatch",
            audience: "BloomWatch",
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, "ghost@example.com"),
                new Claim("display_name", "Ghost"),
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task GetMyProfile_Authenticated_Returns200WithProfile()
    {
        var token = await RegisterAndLoginAsync(displayName: "Alice");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("userId").GetGuid().Should().NotBeEmpty();
        body.GetProperty("email").GetString().Should().Contain("@example.com");
        body.GetProperty("displayName").GetString().Should().Be("Alice");
        body.GetProperty("accountStatus").GetString().Should().Be("Active");
        body.GetProperty("isEmailVerified").GetBoolean().Should().BeFalse();
        body.GetProperty("createdAtUtc").GetDateTime().Should().BeBefore(DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task GetMyProfile_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_NonexistentUser_Returns404()
    {
        var token = GenerateTokenForNonexistentUser();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
