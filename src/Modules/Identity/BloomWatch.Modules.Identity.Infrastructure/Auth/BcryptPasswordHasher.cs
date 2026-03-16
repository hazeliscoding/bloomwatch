using BloomWatch.Modules.Identity.Application.Abstractions;

namespace BloomWatch.Modules.Identity.Infrastructure.Auth;

/// <summary>
/// Implements <see cref="IPasswordHasher"/> using the bcrypt adaptive hashing algorithm.
/// </summary>
/// <remarks>
/// Bcrypt automatically generates and embeds a unique salt per hash, making stored hashes
/// resistant to rainbow-table attacks. The work factor is set to <c>12</c> (2^12 = 4,096 iterations),
/// balancing security against hashing latency. Increase the work factor as hardware improves.
/// </remarks>
internal sealed class BcryptPasswordHasher : IPasswordHasher
{
    /// <summary>
    /// The bcrypt work factor (cost parameter). A value of 12 produces 2^12 key-expansion rounds.
    /// </summary>
    private const int WorkFactor = 12;

    /// <inheritdoc />
    /// <remarks>
    /// Delegates to <see cref="BCrypt.Net.BCrypt.HashPassword(string, int)"/> with a work factor
    /// of <see cref="WorkFactor"/>. The returned string includes the algorithm version, cost, salt,
    /// and hash in the standard bcrypt format (<c>$2a$12$...</c>).
    /// </remarks>
    public string Hash(string plainText)
        => BCrypt.Net.BCrypt.HashPassword(plainText, WorkFactor);

    /// <inheritdoc />
    /// <remarks>
    /// Delegates to <see cref="BCrypt.Net.BCrypt.Verify(string, string)"/>, which extracts the
    /// salt and cost from the stored <paramref name="hash"/> and re-hashes <paramref name="plainText"/>
    /// for comparison.
    /// </remarks>
    public bool Verify(string plainText, string hash)
        => BCrypt.Net.BCrypt.Verify(plainText, hash);
}
