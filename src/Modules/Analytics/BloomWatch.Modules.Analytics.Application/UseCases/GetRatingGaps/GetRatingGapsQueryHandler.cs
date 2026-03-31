using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.Exceptions;
using BloomWatch.Modules.Analytics.Application.Shared;
using BloomWatch.Modules.Analytics.Application.UseCases.GetDashboardSummary;
using MediatR;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetRatingGaps;

public sealed class GetRatingGapsQueryHandler(
    IMembershipChecker membershipChecker,
    IWatchSpaceAnalyticsDataSource dataSource,
    IUserDisplayNameLookup userDisplayNameLookup)
    : IRequestHandler<GetRatingGapsQuery, RatingGapsResult>
{
    public async Task<RatingGapsResult> Handle(
        GetRatingGapsQuery query,
        CancellationToken cancellationToken)
    {
        var isMember = await membershipChecker.IsMemberAsync(
            query.WatchSpaceId, query.UserId, cancellationToken);

        if (!isMember)
            throw new NotAWatchSpaceMemberException();

        var allAnime = await dataSource.GetAnimeWithParticipantsAsync(
            query.WatchSpaceId, cancellationToken);

        var animeGaps = CompatibilityComputer.ComputeAnimeGaps(allAnime);

        if (animeGaps.Count == 0)
            return new RatingGapsResult([], "Not enough data");

        // Secondary sort: tie-break equal gaps by title ascending
        var sorted = animeGaps
            .OrderByDescending(x => x.Gap)
            .ThenBy(x => x.Anime.PreferredTitle, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Batch-resolve display names
        var allRaterIds = sorted
            .SelectMany(g => g.Raters.Select(r => r.UserId))
            .Distinct();

        var displayNames = await userDisplayNameLookup.GetDisplayNamesAsync(
            allRaterIds, cancellationToken);

        var items = sorted
            .Select(g => new RatingGapItem(
                g.Anime.WatchSpaceAnimeId,
                g.Anime.PreferredTitle,
                g.Anime.CoverImageUrl,
                Math.Round(g.Gap, 2),
                g.Raters.Select(r => new RaterResult(
                    r.UserId,
                    displayNames.GetValueOrDefault(r.UserId, "Unknown"),
                    r.RatingScore!.Value)).ToList()))
            .ToList();

        return new RatingGapsResult(items, null);
    }
}
