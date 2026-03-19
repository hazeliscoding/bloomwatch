using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetRatingGaps;

public sealed record RatingGapsResult(
    IReadOnlyList<RatingGapItem> Items,
    string? Message);

public sealed record RatingGapItem(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrl,
    decimal Gap,
    IReadOnlyList<RaterResult> Ratings);
