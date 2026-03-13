using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace BloomWatch.Modules.Identity.UnitTests.Domain;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user+tag@sub.domain.org")]
    public void From_ValidEmail_CreatesEmailAddress(string email)
    {
        var result = EmailAddress.From(email);
        result.Value.Should().Be(email.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    [InlineData("")]
    [InlineData("   ")]
    public void From_InvalidEmail_ThrowsArgumentException(string email)
    {
        var act = () => EmailAddress.From(email);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_SameEmail_AreEqual()
    {
        var a = EmailAddress.From("user@example.com");
        var b = EmailAddress.From("user@example.com");
        a.Should().Be(b);
    }
}
