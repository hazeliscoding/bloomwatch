using BloomWatch.Modules.Analytics.Application.DTOs;
using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;
using FluentAssertions;

namespace BloomWatch.Modules.Analytics.UnitTests.Application;

public sealed class RatingGapComputationTests
{
    [Fact]
    public void ComputePairwiseGap_TwoRaters_ReturnsAbsoluteDifference()
    {
        var raters = new List<ParticipantData>
        {
            new(Guid.NewGuid(), 8.0m),
            new(Guid.NewGuid(), 3.0m)
        };

        var gap = GetDashboardSummaryQueryHandler.ComputePairwiseGap(raters);

        gap.Should().Be(5.0m);
    }

    [Fact]
    public void ComputePairwiseGap_ThreeRaters_ReturnsMeanOfAllPairs()
    {
        // Ratings: 9.0, 5.0, 7.0
        // Pairs: |9-5|=4, |9-7|=2, |5-7|=2 → mean = 8/3 ≈ 2.6667
        var raters = new List<ParticipantData>
        {
            new(Guid.NewGuid(), 9.0m),
            new(Guid.NewGuid(), 5.0m),
            new(Guid.NewGuid(), 7.0m)
        };

        var gap = GetDashboardSummaryQueryHandler.ComputePairwiseGap(raters);

        gap.Should().BeApproximately(2.6667m, 0.001m);
    }

    [Fact]
    public void ComputePairwiseGap_IdenticalRatings_ReturnsZero()
    {
        var raters = new List<ParticipantData>
        {
            new(Guid.NewGuid(), 7.0m),
            new(Guid.NewGuid(), 7.0m)
        };

        var gap = GetDashboardSummaryQueryHandler.ComputePairwiseGap(raters);

        gap.Should().Be(0m);
    }

    [Fact]
    public void ComputeAnimeGaps_ExcludesAnimeWithLessThanTwoRaters()
    {
        var allAnime = new List<WatchSpaceAnimeData>
        {
            MakeAnime("Watching", 5.0m),          // Only 1 rater
            MakeAnime("Watching"),                  // No raters
            MakeAnime("Watching", 8.0m, 3.0m)      // 2 raters
        };

        var gaps = GetDashboardSummaryQueryHandler.ComputeAnimeGaps(allAnime);

        gaps.Should().HaveCount(1);
        gaps[0].Gap.Should().Be(5.0m);
    }

    [Fact]
    public void ComputeAnimeGaps_SortedByGapDescending()
    {
        var allAnime = new List<WatchSpaceAnimeData>
        {
            MakeAnime("Watching", 7.0m, 6.0m),  // gap = 1.0
            MakeAnime("Watching", 9.0m, 2.0m),  // gap = 7.0
            MakeAnime("Watching", 8.0m, 3.0m)   // gap = 5.0
        };

        var gaps = GetDashboardSummaryQueryHandler.ComputeAnimeGaps(allAnime);

        gaps.Should().HaveCount(3);
        gaps[0].Gap.Should().Be(7.0m);
        gaps[1].Gap.Should().Be(5.0m);
        gaps[2].Gap.Should().Be(1.0m);
    }

    [Fact]
    public void ComputeAnimeGaps_NoQualifyingAnime_ReturnsEmpty()
    {
        var allAnime = new List<WatchSpaceAnimeData>
        {
            MakeAnime("Watching", 5.0m),
            MakeAnime("Backlog")
        };

        var gaps = GetDashboardSummaryQueryHandler.ComputeAnimeGaps(allAnime);

        gaps.Should().BeEmpty();
    }

    [Fact]
    public void ComputeAnimeGaps_IgnoresParticipantsWithNullRating()
    {
        var anime = new WatchSpaceAnimeData(
            Guid.NewGuid(), "Test", null, null, null, "Watching", 0, DateTime.UtcNow,
            new List<ParticipantData>
            {
                new(Guid.NewGuid(), 8.0m),
                new(Guid.NewGuid(), null),    // No rating
                new(Guid.NewGuid(), 3.0m)
            });

        var gaps = GetDashboardSummaryQueryHandler.ComputeAnimeGaps(new[] { anime });

        gaps.Should().HaveCount(1);
        gaps[0].Gap.Should().Be(5.0m); // Only the two with ratings count
        gaps[0].Raters.Should().HaveCount(2);
    }

    private static WatchSpaceAnimeData MakeAnime(string status, params decimal[] ratings)
    {
        return new WatchSpaceAnimeData(
            Guid.NewGuid(), "Test Anime", null, null, null, status, 0, DateTime.UtcNow,
            ratings.Select(r => new ParticipantData(Guid.NewGuid(), r)).ToList());
    }
}
