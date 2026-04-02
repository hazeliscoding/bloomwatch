using BloomWatch.Modules.Identity.Domain.ValueObjects;

namespace BloomWatch.Modules.Identity.Domain.Aggregates;

/// <summary>
/// Represents a refresh token issued to a user after successful authentication.
/// Refresh tokens are single-use; each use rotates to a new token.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    // Required by EF Core
    private RefreshToken() { }

    private RefreshToken(Guid id, UserId userId, string tokenHash, DateTime expiresAt, DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        IsRevoked = false;
    }

    public static RefreshToken Create(UserId userId, string tokenHash, DateTime expiresAt)
        => new(Guid.NewGuid(), userId, tokenHash, expiresAt, DateTime.UtcNow);

    public bool IsValid() => !IsRevoked && ExpiresAt > DateTime.UtcNow;

    public void Revoke() => IsRevoked = true;
}
