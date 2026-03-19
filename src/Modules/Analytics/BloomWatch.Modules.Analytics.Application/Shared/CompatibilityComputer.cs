using BloomWatch.Modules.Analytics.Application.DTOs;
using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

namespace BloomWatch.Modules.Analytics.Application.Shared;

/// <summary>
/// Pure computation helpers for compatibility score and per-anime rating gaps.
/// Shared by both the dashboard summary and the dedicated compatibility endpoint.
/// </summary>
internal static class CompatibilityComputer
{
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
