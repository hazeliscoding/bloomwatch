namespace BloomWatch.Modules.Identity.Application.Abstractions;

/// <summary>
/// Provides password hashing and verification operations for secure credential storage.
/// </summary>
/// <remarks>
/// Implementations must use a cryptographically secure, one-way hashing algorithm
/// (such as bcrypt, Argon2, or PBKDF2) with a unique salt per password.
/// The <see cref="Verify"/> method must be constant-time to prevent timing attacks.
/// </remarks>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password for secure storage.
    /// </summary>
    /// <param name="plainText">The plain-text password to hash. Must not be <see langword="null"/> or empty.</param>
    /// <returns>The hashed password string, including the embedded salt.</returns>
    string Hash(string plainText);

    /// <summary>
    /// Verifies a plain-text password against a previously hashed password.
    /// </summary>
    /// <param name="plainText">The plain-text password to verify.</param>
    /// <param name="hash">The stored password hash to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if the plain-text password matches the hash;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool Verify(string plainText, string hash);
}
