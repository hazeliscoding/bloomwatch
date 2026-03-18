using BloomWatch.Modules.Analytics.Application.DTOs;
using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;
using FluentAssertions;

namespace BloomWatch.Modules.Analytics.UnitTests.Application;

public sealed class CompatibilityScoreTests
{
    [Fact]
    public void ComputeCompatibility_PerfectScore_Returns100()
    {
        // All raters gave the same score on every anime → averageGap = 0
        var animeGaps = MakeGaps((0m, 3));

        var (result, message) = GetDashboardSummaryQueryHandler.ComputeCompatibility(animeGaps);

        result.Should().NotBeNull();
        result!.Score.Should().Be(100);
        message.Should().BeNull();
    }

    [Fact]
    public void ComputeCompatibility_HighGap_ReturnsLowScore()
    {
        // averageGap = 6.0 → score = max(0, round(100 - 60)) = 40
        var animeGaps = MakeGaps((6.0m, 2));

        var (result, _) = GetDashboardSummaryQueryHandler.ComputeCompatibility(animeGaps);

        result!.Score.Should().Be(40);
    }

    [Fact]
    public void ComputeCompatibility_VeryLargeGap_ClampsToZero()
    {
        // averageGap = 12.0 → 100 - 120 = -20 → clamped to 0
        var animeGaps = MakeGaps((12.0m, 1));

        var (result, _) = GetDashboardSummaryQueryHandler.ComputeCompatibility(animeGaps);

        result!.Score.Should().Be(0);
    }

    [Fact]
    public void ComputeCompatibility_Rounding_HalfGap()
    {
        // averageGap = 0.5 → 100 - 5 = 95
        var animeGaps = MakeGaps((0.5m, 1));

        var (result, _) = GetDashboardSummaryQueryHandler.ComputeCompatibility(animeGaps);

        result!.Score.Should().Be(95);
    }

    [Fact]
    public void ComputeCompatibility_NoQualifyingAnime_ReturnsNull()
    {
        var animeGaps = new List<(WatchSpaceAnimeData, decimal, List<ParticipantData>)>();

        var (result, message) = GetDashboardSummaryQueryHandler.ComputeCompatibility(animeGaps);

        result.Should().BeNull();
        message.Should().Be("Not enough data");
    }

    [Fact]
    public void ComputeCompatibility_IncludesRatedTogetherCount()
    {
        var animeGaps = MakeGaps((1.0m, 2), (2.0m, 2), (0.5m, 2), (3.0m, 2));

        var (result, _) = GetDashboardSummaryQueryHandler.ComputeCompatibility(animeGaps);

        result!.RatedTogetherCount.Should().Be(4);
    }

    [Fact]
    public void ComputeCompatibility_AverageGapIsRounded()
    {
        // Create gaps of 1.333... and 2.666... → average = 2.0
        var animeGaps = MakeGaps((1.333m, 1), (2.667m, 1));

        var (result, _) = GetDashboardSummaryQueryHandler.ComputeCompatibility(animeGaps);

        result!.AverageGap.Should().Be(2.0m);
    }

    private static List<(WatchSpaceAnimeData Anime, decimal Gap, List<ParticipantData> Raters)> MakeGaps(
        params (decimal gap, int raterCount)[] entries)
    {
        return entries.Select(e =>
        {
            var anime = new WatchSpaceAnimeData(
                Guid.NewGuid(), "Test", null, null, null, "Watching", 0, DateTime.UtcNow,
                Enumerable.Range(0, e.raterCount).Select(_ =>
                    new ParticipantData(Guid.NewGuid(), 5.0m)).ToList());
            return (anime, e.gap, anime.Participants.ToList());
        }).ToList();
    }
}
