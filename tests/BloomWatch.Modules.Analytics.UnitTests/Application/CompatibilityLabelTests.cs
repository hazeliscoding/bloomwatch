using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;
using FluentAssertions;

namespace BloomWatch.Modules.Analytics.UnitTests.Application;

public sealed class CompatibilityLabelTests
{
    [Theory]
    [InlineData(100, "Very synced, with a little spice")]
    [InlineData(95, "Very synced, with a little spice")]
    [InlineData(90, "Very synced, with a little spice")]
    [InlineData(89, "Pretty aligned")]
    [InlineData(75, "Pretty aligned")]
    [InlineData(70, "Pretty aligned")]
    [InlineData(69, "Some differences")]
    [InlineData(55, "Some differences")]
    [InlineData(50, "Some differences")]
    [InlineData(49, "Wildly different tastes")]
    [InlineData(30, "Wildly different tastes")]
    [InlineData(0, "Wildly different tastes")]
    public void GetCompatibilityLabel_ReturnsCorrectLabel(int score, string expectedLabel)
    {
        var label = GetDashboardSummaryQueryHandler.GetCompatibilityLabel(score);

        label.Should().Be(expectedLabel);
    }
}
