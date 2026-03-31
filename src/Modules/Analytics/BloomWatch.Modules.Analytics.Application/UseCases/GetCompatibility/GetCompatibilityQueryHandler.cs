using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.Exceptions;
using BloomWatch.Modules.Analytics.Application.Shared;
using MediatR;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetCompatibility;

public sealed class GetCompatibilityQueryHandler(
    IMembershipChecker membershipChecker,
    IWatchSpaceAnalyticsDataSource dataSource)
    : IRequestHandler<GetCompatibilityQuery, CompatibilityScoreResult>
{
    public async Task<CompatibilityScoreResult> Handle(
        GetCompatibilityQuery query,
        CancellationToken cancellationToken)
    {
        var isMember = await membershipChecker.IsMemberAsync(
            query.WatchSpaceId, query.UserId, cancellationToken);

        if (!isMember)
            throw new NotAWatchSpaceMemberException();

        var allAnime = await dataSource.GetAnimeWithParticipantsAsync(
            query.WatchSpaceId, cancellationToken);

        var animeGaps = CompatibilityComputer.ComputeAnimeGaps(allAnime);
        var (compatibility, message) = CompatibilityComputer.ComputeCompatibility(animeGaps);

        return new CompatibilityScoreResult(compatibility, message);
    }
}
