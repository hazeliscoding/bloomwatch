using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace BloomWatch.Modules.Identity.UnitTests.Domain;

public sealed class RefreshTokenTests
{
    private static RefreshToken MakeToken(bool isRevoked = false, int expiresInSeconds = 3600)
    {
        var userId = UserId.New();
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        var token = RefreshToken.Create(userId, "hash-abc", expiresAt);
        if (isRevoked) token.Revoke();
        return token;
    }

    [Fact]
    public void IsValid_NonRevokedNonExpired_ReturnsTrue()
    {
        var token = MakeToken();
        token.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_Revoked_ReturnsFalse()
    {
        var token = MakeToken(isRevoked: true);
        token.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_Expired_ReturnsFalse()
    {
        var token = MakeToken(isRevoked: false, expiresInSeconds: -1);
        token.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_RevokedAndExpired_ReturnsFalse()
    {
        var token = MakeToken(isRevoked: true, expiresInSeconds: -1);
        token.IsValid().Should().BeFalse();
    }

    [Fact]
    public void Revoke_SetsIsRevokedTrue()
    {
        var token = MakeToken();
        token.Revoke();
        token.IsRevoked.Should().BeTrue();
    }
}
