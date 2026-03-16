using BloomWatch.Modules.Identity.Domain.Aggregates;

namespace BloomWatch.Modules.Identity.Application.Abstractions;

/// <summary>
/// Represents the result of a JWT token generation operation.
/// </summary>
/// <param name="AccessToken">The signed JWT access token string.</param>
/// <param name="ExpiresAt">The UTC date and time at which the token expires.</param>
public sealed record TokenResult(string AccessToken, DateTime ExpiresAt);

/// <summary>
/// Generates signed JWT access tokens for authenticated users.
/// </summary>
/// <remarks>
/// Implementations are responsible for encoding user claims (such as user ID and email)
/// into the token payload and signing the token with the configured secret or key.
/// </remarks>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT access token containing claims derived from the specified user.
    /// </summary>
    /// <param name="user">The authenticated user for whom to generate the token.</param>
    /// <returns>A <see cref="TokenResult"/> containing the signed access token and its expiration time.</returns>
    TokenResult GenerateToken(User user);
}
