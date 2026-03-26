using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.Exceptions;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetSharedStats;

public sealed class GetSharedStatsQueryHandler(
    IMembershipChecker membershipChecker,
    IWatchSpaceAnalyticsDataSource dataSource)
{
    public async Task<SharedStatsResult> HandleAsync(
        GetSharedStatsQuery query,
        CancellationToken cancellationToken = default)
    {
        var isMember = await membershipChecker.IsMemberAsync(
            query.WatchSpaceId, query.UserId, cancellationToken);

        if (!isMember)
            throw new NotAWatchSpaceMemberException();

        var allAnime = await dataSource.GetAnimeWithParticipantsAsync(
            query.WatchSpaceId, cancellationToken);

        var totalEpisodes = allAnime.Sum(a => a.SharedEpisodesWatched);
        var totalFinished = allAnime.Count(a =>
            a.SharedStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase));
        var totalDropped = allAnime.Count(a =>
            a.SharedStatus.Equals("Dropped", StringComparison.OrdinalIgnoreCase));

        return new SharedStatsResult(
            totalEpisodes,
            totalFinished,
            totalDropped);
    }
}
