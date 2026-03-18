namespace BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

public sealed record DashboardSummaryResult(
    DashboardStatsResult Stats,
    CompatibilityResult? Compatibility,
    string? CompatibilityMessage,
    IReadOnlyList<CurrentlyWatchingItemResult> CurrentlyWatching,
    IReadOnlyList<BacklogHighlightResult> BacklogHighlights,
    IReadOnlyList<RatingGapHighlightResult> RatingGapHighlights);

public sealed record DashboardStatsResult(
    int TotalShows,
    int CurrentlyWatching,
    int Finished,
    int EpisodesWatchedTogether);

public sealed record CompatibilityResult(
    int Score,
    decimal AverageGap,
    int RatedTogetherCount,
    string Label);

public sealed record CurrentlyWatchingItemResult(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrl,
    int SharedEpisodesWatched,
    int? EpisodeCountSnapshot);

public sealed record BacklogHighlightResult(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrl,
    string? Format);

public sealed record RatingGapHighlightResult(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrl,
    decimal Gap,
    IReadOnlyList<RaterResult> Ratings);

public sealed record RaterResult(
    Guid UserId,
    string DisplayName,
    decimal RatingScore);
