using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace BloomWatch.Modules.Identity.UnitTests.Domain;

public sealed class PasswordResetTokenTests
{
    private static PasswordResetToken MakeToken(bool isUsed = false, int expiresInSeconds = 3600)
    {
        var userId = UserId.New();
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        var token = PasswordResetToken.Create(userId, "hash-abc", expiresAt);
        if (isUsed) token.MarkUsed();
        return token;
    }

    [Fact]
    public void IsValid_UnusedAndNonExpired_ReturnsTrue()
    {
        var token = MakeToken();
        token.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_UsedToken_ReturnsFalse()
    {
        var token = MakeToken(isUsed: true);
        token.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_ExpiredToken_ReturnsFalse()
    {
        var token = MakeToken(isUsed: false, expiresInSeconds: -1);
        token.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_UsedAndExpired_ReturnsFalse()
    {
        var token = MakeToken(isUsed: true, expiresInSeconds: -1);
        token.IsValid().Should().BeFalse();
    }

    [Fact]
    public void MarkUsed_SetsIsUsedTrue()
    {
        var token = MakeToken();
        token.MarkUsed();
        token.IsUsed.Should().BeTrue();
    }
}
