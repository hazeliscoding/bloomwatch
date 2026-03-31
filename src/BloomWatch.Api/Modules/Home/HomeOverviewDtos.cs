using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Api.Modules.Home;

/// <summary>
/// Query to retrieve the home overview for the authenticated user.
/// </summary>
/// <param name="UserId">The authenticated user's identifier.</param>
public sealed record GetHomeOverviewQuery(Guid UserId) : IQuery<HomeOverviewResult>;

/// <summary>
/// Composite result for the home overview endpoint.
/// </summary>
public sealed record HomeOverviewResult(
    string DisplayName,
    HomeStatsResult Stats,
    IReadOnlyList<HomeWatchSpaceSummary> WatchSpaces,
    IReadOnlyList<HomeRecentActivityItem> RecentActivity);

/// <summary>
/// Aggregated stats across all the user's watch spaces.
/// </summary>
public sealed record HomeStatsResult(
    int WatchSpaceCount,
    int TotalAnimeTracked,
    int TotalEpisodesWatchedTogether);

/// <summary>
/// A watch space summary enriched with per-space anime counts.
/// </summary>
public sealed record HomeWatchSpaceSummary(
    Guid WatchSpaceId,
    string Name,
    string Role,
    int MemberCount,
    IReadOnlyList<HomeMemberPreview> MemberPreviews,
    int WatchingCount,
    int BacklogCount);

/// <summary>
/// Lightweight member preview for avatar display.
/// </summary>
public sealed record HomeMemberPreview(string DisplayName);

/// <summary>
/// A recently updated anime across any of the user's watch spaces.
/// </summary>
public sealed record HomeRecentActivityItem(
    Guid WatchSpaceAnimeId,
    Guid WatchSpaceId,
    string WatchSpaceName,
    string PreferredTitle,
    string? CoverImageUrl,
    string SharedStatus,
    DateTime LastUpdatedAt);
