using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BloomWatch.Modules.Identity.Infrastructure.Auth;

/// <summary>
/// Implements <see cref="IJwtTokenGenerator"/> using HMAC-SHA256 symmetric signing.
/// </summary>
/// <remarks>
/// <para>
/// Reads JWT configuration from the <c>Identity:Jwt</c> configuration section:
/// </para>
/// <list type="bullet">
///   <item><description><c>SecretKey</c> (required) -- the HMAC-SHA256 signing key.</description></item>
///   <item><description><c>Issuer</c> (optional, defaults to <c>"BloomWatch"</c>).</description></item>
///   <item><description><c>Audience</c> (optional, defaults to <c>"BloomWatch"</c>).</description></item>
/// </list>
/// <para>
/// Generated tokens include the following claims: <c>sub</c> (user ID), <c>email</c>,
/// <c>display_name</c>, and <c>iat</c> (issued-at). Tokens expire one hour after creation.
/// </para>
/// </remarks>
internal sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenGenerator"/> class.
    /// </summary>
    /// <param name="configuration">
    /// The application configuration, which must contain an <c>Identity:Jwt:SecretKey</c> entry.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>Identity:Jwt:SecretKey</c> is missing or <see langword="null"/>.
    /// </exception>
    public JwtTokenGenerator(IConfiguration configuration)
    {
        _secretKey = configuration["Identity:Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key is not configured (Identity:Jwt:SecretKey).");
        _issuer = configuration["Identity:Jwt:Issuer"] ?? "BloomWatch";
        _audience = configuration["Identity:Jwt:Audience"] ?? "BloomWatch";
    }

    /// <inheritdoc />
    /// <remarks>
    /// Creates a JWT with a one-hour lifetime signed using HMAC-SHA256. The token payload
    /// includes the user's ID (<c>sub</c>), email, display name, and the issued-at timestamp.
    /// </remarks>
    public TokenResult GenerateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim("display_name", user.DisplayName.Value),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResult(accessToken, expiresAt);
    }
}
