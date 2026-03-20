using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetRatingGaps;

/// <summary>
/// Result containing all anime with rating divergence between watch space members.
/// </summary>
/// <param name="Items">Rating gap entries sorted by descending gap magnitude, with alphabetical title tie-breaking.</param>
/// <param name="Message">A human-readable explanation when no gaps are available (e.g., "No anime have been rated by multiple members yet").</param>
public sealed record RatingGapsResult(
    IReadOnlyList<RatingGapItem> Items,
    string? Message);

/// <summary>
/// A single anime entry where members' ratings diverge.
/// </summary>
/// <param name="WatchSpaceAnimeId">The tracked anime's unique identifier within the watch space.</param>
/// <param name="PreferredTitle">The display title.</param>
/// <param name="CoverImageUrl">The cover image URL, or <c>null</c>.</param>
/// <param name="Gap">The absolute difference between the highest and lowest member ratings.</param>
/// <param name="Ratings">Individual member ratings for this anime.</param>
public sealed record RatingGapItem(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrl,
    decimal Gap,
    IReadOnlyList<RaterResult> Ratings);
