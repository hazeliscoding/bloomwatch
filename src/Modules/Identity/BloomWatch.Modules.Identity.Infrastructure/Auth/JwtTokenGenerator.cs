using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Domain.Aggregates;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BloomWatch.Modules.Identity.Infrastructure.Auth;

internal sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _secretKey = configuration["Identity:Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key is not configured (Identity:Jwt:SecretKey).");
        _issuer = configuration["Identity:Jwt:Issuer"] ?? "BloomWatch";
        _audience = configuration["Identity:Jwt:Audience"] ?? "BloomWatch";
    }

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
