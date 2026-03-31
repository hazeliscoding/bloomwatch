using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using MediatR;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.ListWatchSpaceAnime;

/// <summary>
/// Handles <see cref="ListWatchSpaceAnimeQuery"/> by verifying membership
/// and returning the list of tracked anime with participant summaries.
/// </summary>
public sealed class ListWatchSpaceAnimeQueryHandler(
    IMembershipChecker membershipChecker,
    IAnimeTrackingRepository repository)
    : IRequestHandler<ListWatchSpaceAnimeQuery, ListWatchSpaceAnimeResult>
{
    public async Task<ListWatchSpaceAnimeResult> Handle(
        ListWatchSpaceAnimeQuery query,
        CancellationToken cancellationToken)
    {
        var isMember = await membershipChecker.IsMemberAsync(
            query.WatchSpaceId, query.RequestingUserId, cancellationToken);

        if (!isMember)
            throw new NotAWatchSpaceMemberException();

        var animeList = await repository.ListByWatchSpaceAsync(
            query.WatchSpaceId, query.Status, cancellationToken);

        var items = animeList.Select(a => new WatchSpaceAnimeListItem(
            a.Id.Value,
            a.AniListMediaId,
            a.PreferredTitle,
            a.CoverImageUrlSnapshot,
            a.EpisodeCountSnapshot,
            a.SharedStatus.ToString(),
            a.SharedEpisodesWatched,
            a.AddedAtUtc,
            a.ParticipantEntries.Select(p => new ParticipantSummary(
                p.UserId,
                p.IndividualStatus.ToString(),
                p.EpisodesWatched)).ToList())).ToList();

        return new ListWatchSpaceAnimeResult(items);
    }
}
