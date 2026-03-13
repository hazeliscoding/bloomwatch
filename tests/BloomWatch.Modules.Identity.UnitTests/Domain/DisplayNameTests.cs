using BloomWatch.Modules.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace BloomWatch.Modules.Identity.UnitTests.Domain;

public sealed class DisplayNameTests
{
    [Theory]
    [InlineData("Alice")]
    [InlineData("  Bob  ")]
    [InlineData("かおり")]
    public void From_ValidName_CreatesDisplayName(string name)
    {
        var result = DisplayName.From(name);
        result.Value.Should().Be(name.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void From_BlankOrNull_ThrowsArgumentException(string? name)
    {
        var act = () => DisplayName.From(name!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_TooLong_ThrowsArgumentException()
    {
        var longName = new string('a', DisplayName.MaxLength + 1);
        var act = () => DisplayName.From(longName);
        act.Should().Throw<ArgumentException>();
    }
}
