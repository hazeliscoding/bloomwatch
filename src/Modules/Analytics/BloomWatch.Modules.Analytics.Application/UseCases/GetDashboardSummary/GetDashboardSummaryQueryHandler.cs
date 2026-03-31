using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.DTOs;
using BloomWatch.Modules.Analytics.Application.Exceptions;
using BloomWatch.Modules.Analytics.Application.Shared;
using MediatR;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;

/// <summary>
/// Handles <see cref="GetDashboardSummaryQuery"/> by verifying membership,
/// loading anime data from the AnimeTracking module, and computing the dashboard summary.
/// </summary>
public sealed class GetDashboardSummaryQueryHandler(
    IMembershipChecker membershipChecker,
    IWatchSpaceAnalyticsDataSource dataSource,
    IUserDisplayNameLookup userDisplayNameLookup)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResult>
{
    private const int MaxCurrentlyWatching = 5;
    private const int MaxBacklogHighlights = 5;
    private const int MaxRatingGapHighlights = 3;

    public async Task<DashboardSummaryResult> Handle(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken)
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
        var animeGaps = CompatibilityComputer.ComputeAnimeGaps(allAnime);

        var ratingGapHighlights = await BuildRatingGapHighlights(
            animeGaps, allAnime, cancellationToken);

        var (compatibility, compatibilityMessage) = CompatibilityComputer.ComputeCompatibility(animeGaps);

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
}
