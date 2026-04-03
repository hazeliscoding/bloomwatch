using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Domain.Aggregates;

/// <summary>
/// Represents a single-use, time-limited token for resetting a user's password.
/// Only the SHA-256 hash of the plain token is stored — the plain token travels only via email.
/// </summary>
public sealed class PasswordResetToken
{
    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsUsed { get; private set; }

    // Required by EF Core
    private PasswordResetToken() { }

    private PasswordResetToken(Guid id, UserId userId, string tokenHash, DateTime expiresAt, DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        IsUsed = false;
    }

    public static PasswordResetToken Create(UserId userId, string tokenHash, DateTime expiresAt)
        => new(Guid.NewGuid(), userId, tokenHash, expiresAt, DateTime.UtcNow);

    /// <summary>Returns true only when the token has not been used and has not expired.</summary>
    public bool IsValid() => !IsUsed && ExpiresAt > DateTime.UtcNow;

    /// <summary>Marks the token as used, preventing further reuse.</summary>
    public void MarkUsed() => IsUsed = true;
}
