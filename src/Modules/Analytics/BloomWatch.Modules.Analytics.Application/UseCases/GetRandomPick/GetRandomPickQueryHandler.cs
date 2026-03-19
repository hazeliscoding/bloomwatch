using BloomWatch.Modules.Analytics.Application.Abstractions;
using BloomWatch.Modules.Analytics.Application.Exceptions;

namespace BloomWatch.Modules.Analytics.Application.UseCases.GetRandomPick;

public sealed class GetRandomPickQueryHandler(
    IMembershipChecker membershipChecker,
    IWatchSpaceAnalyticsDataSource dataSource)
{
    public async Task<RandomPickResult> HandleAsync(
        GetRandomPickQuery query,
        CancellationToken cancellationToken = default)
    {
        var isMember = await membershipChecker.IsMemberAsync(
            query.WatchSpaceId, query.UserId, cancellationToken);

        if (!isMember)
            throw new NotAWatchSpaceMemberException();

        var allAnime = await dataSource.GetAnimeWithParticipantsAsync(
            query.WatchSpaceId, cancellationToken);

        var picked = allAnime
            .Where(a => a.SharedStatus.Equals("Backlog", StringComparison.OrdinalIgnoreCase))
            .OrderBy(_ => Guid.NewGuid())
            .FirstOrDefault();

        if (picked is null)
            return new RandomPickResult(null, "Backlog is empty");

        return new RandomPickResult(
            new RandomPickAnimeResult(
                picked.WatchSpaceAnimeId,
                picked.PreferredTitle,
                picked.CoverImageUrl,
                picked.EpisodeCountSnapshot,
                picked.Mood,
                picked.Vibe,
                picked.Pitch),
            null);
    }
}
