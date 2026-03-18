using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.DTOs;
using BloomWatch.Modules.Analytics.Application.Exceptions;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

/// <summary>
/// Handles <see cref="GetDashboardSummaryQuery"/> by verifying membership,
/// loading anime data from the AnimeTracking module, and computing the dashboard summary.
/// </summary>
public sealed class GetDashboardSummaryQueryHandler(
    IMembershipChecker membershipChecker,
    IWatchSpaceAnalyticsDataSource dataSource,
    IUserDisplayNameLookup userDisplayNameLookup)
{
    private const int MaxCurrentlyWatching = 5;
    private const int MaxBacklogHighlights = 5;
    private const int MaxRatingGapHighlights = 3;

    public async Task<DashboardSummaryResult> HandleAsync(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var isMember = await membershipChecker.IsMemberAsync(
            query.WatchSpaceId, query.UserId, cancellationToken);

        if (!isMember)
            throw new NotAWatchSpaceMemberException();

        var allAnime = await dataSource.GetAnimeWithParticipantsAsync(
            query.WatchSpaceId, cancellationToken);

        var stats = ComputeStats(allAnime);
        var currentlyWatching = ComputeCurrentlyWatching(allAnime);
        var backlogHighlights = ComputeBacklogHighlights(allAnime);

        // Compute per-anime gaps (shared between rating gap highlights and compatibility)
        var animeGaps = ComputeAnimeGaps(allAnime);

        var ratingGapHighlights = await BuildRatingGapHighlights(
            animeGaps, allAnime, cancellationToken);

        var (compatibility, compatibilityMessage) = ComputeCompatibility(animeGaps);

        return new DashboardSummaryResult(
            stats,
            compatibility,
            compatibilityMessage,
            currentlyWatching,
            backlogHighlights,
            ratingGapHighlights);
    }

    private static DashboardStatsResult ComputeStats(IReadOnlyList<WatchSpaceAnimeData> allAnime)
    {
        var totalShows = allAnime.Count;
        var watching = allAnime.Count(a => a.SharedStatus == "Watching");
        var finished = allAnime.Count(a => a.SharedStatus == "Finished");
        var episodesWatchedTogether = allAnime.Sum(a => a.SharedEpisodesWatched);

        return new DashboardStatsResult(totalShows, watching, finished, episodesWatchedTogether);
    }

    private static IReadOnlyList<CurrentlyWatchingItemResult> ComputeCurrentlyWatching(
        IReadOnlyList<WatchSpaceAnimeData> allAnime)
    {
        return allAnime
            .Where(a => a.SharedStatus == "Watching")
            .OrderByDescending(a => a.AddedAtUtc)
            .Take(MaxCurrentlyWatching)
            .Select(a => new CurrentlyWatchingItemResult(
                a.WatchSpaceAnimeId,
                a.PreferredTitle,
                a.CoverImageUrl,
                a.SharedEpisodesWatched,
                a.EpisodeCountSnapshot))
            .ToList();
    }

    private static IReadOnlyList<BacklogHighlightResult> ComputeBacklogHighlights(
        IReadOnlyList<WatchSpaceAnimeData> allAnime)
    {
        return allAnime
            .Where(a => a.SharedStatus == "Backlog")
            .OrderBy(_ => Guid.NewGuid())
            .Take(MaxBacklogHighlights)
            .Select(a => new BacklogHighlightResult(
                a.WatchSpaceAnimeId,
                a.PreferredTitle,
                a.CoverImageUrl,
                a.Format))
            .ToList();
    }

    /// <summary>
    /// For each anime with 2+ raters, computes the mean absolute pairwise rating gap.
    /// Returns list of (anime, gap, rated participants) sorted by gap descending.
    /// </summary>
    internal static List<(WatchSpaceAnimeData Anime, decimal Gap, List<ParticipantData> Raters)>
        ComputeAnimeGaps(IReadOnlyList<WatchSpaceAnimeData> allAnime)
    {
        var results = new List<(WatchSpaceAnimeData Anime, decimal Gap, List<ParticipantData> Raters)>();

        foreach (var anime in allAnime)
        {
            var raters = anime.Participants
                .Where(p => p.RatingScore.HasValue)
                .ToList();

            if (raters.Count < 2)
                continue;

            var gap = ComputePairwiseGap(raters);
            results.Add((anime, gap, raters));
        }

        return results.OrderByDescending(x => x.Gap).ToList();
    }

    /// <summary>
    /// Computes the mean of absolute differences between all distinct pairs of raters.
    /// </summary>
    internal static decimal ComputePairwiseGap(List<ParticipantData> raters)
    {
        decimal totalDiff = 0;
        int pairCount = 0;

        for (int i = 0; i < raters.Count; i++)
        {
            for (int j = i + 1; j < raters.Count; j++)
            {
                totalDiff += Math.Abs(raters[i].RatingScore!.Value - raters[j].RatingScore!.Value);
                pairCount++;
            }
        }

        return pairCount > 0 ? totalDiff / pairCount : 0;
    }

    private async Task<IReadOnlyList<RatingGapHighlightResult>> BuildRatingGapHighlights(
        List<(WatchSpaceAnimeData Anime, decimal Gap, List<ParticipantData> Raters)> animeGaps,
        IReadOnlyList<WatchSpaceAnimeData> allAnime,
        CancellationToken cancellationToken)
    {
        var topGaps = animeGaps.Take(MaxRatingGapHighlights).ToList();

        if (topGaps.Count == 0)
            return [];

        // Collect all rater user IDs to resolve display names in one batch
        var allRaterIds = topGaps
            .SelectMany(g => g.Raters.Select(r => r.UserId))
            .Distinct();

        var displayNames = await userDisplayNameLookup.GetDisplayNamesAsync(
            allRaterIds, cancellationToken);

        return topGaps
            .Select(g => new RatingGapHighlightResult(
                g.Anime.WatchSpaceAnimeId,
                g.Anime.PreferredTitle,
                g.Anime.CoverImageUrl,
                Math.Round(g.Gap, 2),
                g.Raters.Select(r => new RaterResult(
                    r.UserId,
                    displayNames.GetValueOrDefault(r.UserId, "Unknown"),
                    r.RatingScore!.Value)).ToList()))
            .ToList();
    }

    internal static (CompatibilityResult? Compatibility, string? Message) ComputeCompatibility(
        List<(WatchSpaceAnimeData Anime, decimal Gap, List<ParticipantData> Raters)> animeGaps)
    {
        if (animeGaps.Count == 0)
            return (null, "Not enough data");

        var averageGap = animeGaps.Average(g => g.Gap);
        var score = Math.Max(0, (int)Math.Round(100 - averageGap * 10));
        var label = GetCompatibilityLabel(score);

        return (new CompatibilityResult(score, Math.Round(averageGap, 2), animeGaps.Count, label), null);
    }

    internal static string GetCompatibilityLabel(int score) => score switch
    {
        >= 90 => "Very synced, with a little spice",
        >= 70 => "Pretty aligned",
        >= 50 => "Some differences",
        _ => "Wildly different tastes"
    };
}
