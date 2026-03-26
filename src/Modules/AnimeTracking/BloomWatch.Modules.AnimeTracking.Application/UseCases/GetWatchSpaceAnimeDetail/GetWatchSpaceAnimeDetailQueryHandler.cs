using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.GetWatchSpaceAnimeDetail;

/// <summary>
/// Handles <see cref="GetWatchSpaceAnimeDetailQuery"/> by verifying membership
/// and returning full detail for a single tracked anime.
/// </summary>
public sealed class GetWatchSpaceAnimeDetailQueryHandler(
    IMembershipChecker membershipChecker,
    IAnimeTrackingRepository repository)
{
    public async Task<GetWatchSpaceAnimeDetailResult?> HandleAsync(
        GetWatchSpaceAnimeDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        var isMember = await membershipChecker.IsMemberAsync(
            query.WatchSpaceId, query.RequestingUserId, cancellationToken);

        if (!isMember)
            throw new NotAWatchSpaceMemberException();

        var anime = await repository.GetByIdAsync(
            query.WatchSpaceId,
            WatchSpaceAnimeId.From(query.WatchSpaceAnimeId),
            cancellationToken);

        if (anime is null)
            return null;

        return new GetWatchSpaceAnimeDetailResult(
            anime.Id.Value,
            anime.AniListMediaId,
            anime.PreferredTitle,
            anime.CoverImageUrlSnapshot,
            anime.EpisodeCountSnapshot,
            anime.Format,
            anime.Season,
            anime.SeasonYear,
            anime.SharedStatus.ToString(),
            anime.SharedEpisodesWatched,
            anime.Mood,
            anime.Vibe,
            anime.Pitch,
            anime.AddedByUserId,
            anime.AddedAtUtc,
            anime.ParticipantEntries.Select(p => new ParticipantDetail(
                p.UserId,
                p.IndividualStatus.ToString(),
                p.EpisodesWatched,
                p.RatingScore,
                p.RatingNotes,
                p.LastUpdatedAtUtc)).ToList());
    }
}
