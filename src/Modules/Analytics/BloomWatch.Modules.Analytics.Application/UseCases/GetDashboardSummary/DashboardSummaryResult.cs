namespace BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

/// <summary>
/// Composite dashboard view for a watch space, combining aggregate stats with curated highlight lists.
/// Designed as a single-call endpoint so the frontend can render the full dashboard without multiple requests.
/// </summary>
/// <param name="Stats">Aggregate counters for the watch space.</param>
/// <param name="Compatibility">The taste-compatibility result between members, or <c>null</c> if insufficient data (fewer than 2 members with shared ratings).</param>
/// <param name="CompatibilityMessage">A human-readable explanation when compatibility cannot be computed.</param>
/// <param name="CurrentlyWatching">Anime the group is actively watching (status = Watching), ordered by most recently updated.</param>
/// <param name="BacklogHighlights">A curated selection of anime from the backlog to surface for consideration.</param>
/// <param name="RatingGapHighlights">Anime with the largest rating divergence between members, useful for sparking discussion.</param>
public sealed record DashboardSummaryResult(
    DashboardStatsResult Stats,
    CompatibilityResult? Compatibility,
    string? CompatibilityMessage,
    IReadOnlyList<CurrentlyWatchingItemResult> CurrentlyWatching,
    IReadOnlyList<BacklogHighlightResult> BacklogHighlights,
    IReadOnlyList<RatingGapHighlightResult> RatingGapHighlights);

/// <summary>
/// Aggregate counters summarizing the watch space's overall tracking state.
/// </summary>
/// <param name="TotalShows">Total number of anime tracked in the watch space across all statuses.</param>
/// <param name="CurrentlyWatching">Number of anime with shared status "Watching".</param>
/// <param name="Finished">Number of anime with shared status "Finished".</param>
/// <param name="EpisodesWatchedTogether">Sum of shared episodes watched across all tracked anime.</param>
public sealed record DashboardStatsResult(
    int TotalShows,
    int CurrentlyWatching,
    int Finished,
    int EpisodesWatchedTogether);

/// <summary>
/// Taste-compatibility result computed from members' overlapping anime ratings.
/// </summary>
/// <param name="Score">Compatibility score from 0 (totally different tastes) to 100 (identical ratings).</param>
/// <param name="AverageGap">The average absolute rating difference across all shared rated anime.</param>
/// <param name="RatedTogetherCount">The number of anime that at least two members have rated.</param>
/// <param name="Label">A human-readable label for the compatibility level (e.g., "Soulmates", "Rivals").</param>
public sealed record CompatibilityResult(
    int Score,
    decimal AverageGap,
    int RatedTogetherCount,
    string Label);

/// <summary>
/// An anime the group is currently watching, surfaced on the dashboard for quick access.
/// </summary>
/// <param name="WatchSpaceAnimeId">The tracked anime's unique identifier within the watch space.</param>
/// <param name="PreferredTitle">The display title (English preferred, falling back to romaji).</param>
/// <param name="CoverImageUrl">The anime's cover image URL, or <c>null</c> if unavailable.</param>
/// <param name="SharedEpisodesWatched">The number of episodes the group has collectively reached.</param>
/// <param name="EpisodeCountSnapshot">The total episode count from the AniList snapshot, or <c>null</c> if unknown/ongoing.</param>
public sealed record CurrentlyWatchingItemResult(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrl,
    int SharedEpisodesWatched,
    int? EpisodeCountSnapshot);

/// <summary>
/// A backlog anime surfaced as a highlight to help the group decide what to watch next.
/// </summary>
/// <param name="WatchSpaceAnimeId">The tracked anime's unique identifier within the watch space.</param>
/// <param name="PreferredTitle">The display title.</param>
/// <param name="CoverImageUrl">The cover image URL, or <c>null</c>.</param>
/// <param name="Format">The media format (e.g., "TV", "MOVIE"), or <c>null</c>.</param>
public sealed record BacklogHighlightResult(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrl,
    string? Format);

/// <summary>
/// An anime where members have notably different ratings, surfaced to spark discussion.
/// </summary>
/// <param name="WatchSpaceAnimeId">The tracked anime's unique identifier within the watch space.</param>
/// <param name="PreferredTitle">The display title.</param>
/// <param name="CoverImageUrl">The cover image URL, or <c>null</c>.</param>
/// <param name="Gap">The absolute difference between the highest and lowest member ratings.</param>
/// <param name="Ratings">Individual member ratings for this anime.</param>
public sealed record RatingGapHighlightResult(
    Guid WatchSpaceAnimeId,
    string PreferredTitle,
    string? CoverImageUrl,
    decimal Gap,
    IReadOnlyList<RaterResult> Ratings);

/// <summary>
/// A single member's rating entry used in compatibility and gap calculations.
/// </summary>
/// <param name="UserId">The rating member's unique identifier.</param>
/// <param name="DisplayName">The rating member's display name.</param>
/// <param name="RatingScore">The member's numeric rating score.</param>
public sealed record RaterResult(
    Guid UserId,
    string DisplayName,
    decimal RatingScore);
