namespace BloomWatch.Modules.Identity.Application.Abstractions;

/// <summary>
/// Generates and hashes opaque refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Generates a cryptographically random 32-byte base64url token.</summary>
    string GenerateToken();

    /// <summary>Returns the SHA-256 hex digest of the given plain token.</summary>
    string HashToken(string token);
}
