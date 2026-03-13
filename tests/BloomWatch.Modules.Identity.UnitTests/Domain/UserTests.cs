using BloomWatch.Modules.Identity.Domain.Aggregates;
using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace BloomWatch.Modules.Identity.UnitTests.Domain;

public sealed class UserTests
{
    [Fact]
    public void Register_ValidInput_SetsAllFieldsCorrectly()
    {
        var email = EmailAddress.From("user@example.com");
        var displayName = DisplayName.From("Alice");
        const string passwordHash = "$2a$12$hashedpassword";

        var user = User.Register(email, passwordHash, displayName);

        user.Id.Value.Should().NotBeEmpty();
        user.Email.Should().Be(email);
        user.DisplayName.Should().Be(displayName);
        user.PasswordHash.Should().Be(passwordHash);
        user.AccountStatus.Should().Be(AccountStatus.Active);
        user.IsEmailVerified.Should().BeFalse();
        user.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Register_EmptyPasswordHash_ThrowsArgumentException()
    {
        var email = EmailAddress.From("user@example.com");
        var displayName = DisplayName.From("Alice");

        var act = () => User.Register(email, string.Empty, displayName);
        act.Should().Throw<ArgumentException>();
    }
}
