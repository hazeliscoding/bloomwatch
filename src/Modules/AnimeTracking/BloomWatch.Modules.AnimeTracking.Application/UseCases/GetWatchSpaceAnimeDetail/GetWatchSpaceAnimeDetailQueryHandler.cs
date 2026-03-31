using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using MediatR;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.GetWatchSpaceAnimeDetail;

/// <summary>
/// Handles <see cref="GetWatchSpaceAnimeDetailQuery"/> by verifying membership
/// and returning full detail for a single tracked anime.
/// </summary>
public sealed class GetWatchSpaceAnimeDetailQueryHandler(
    IMembershipChecker membershipChecker,
    IAnimeTrackingRepository repository,
    IMediaCacheLookup mediaCacheLookup)
    : IRequestHandler<GetWatchSpaceAnimeDetailQuery, GetWatchSpaceAnimeDetailResult?>
{
    public async Task<GetWatchSpaceAnimeDetailResult?> Handle(
        GetWatchSpaceAnimeDetailQuery query,
        CancellationToken cancellationToken)
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

        var mediaCache = await mediaCacheLookup.GetByAnilistMediaIdAsync(
            anime.AniListMediaId, cancellationToken);

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
                p.LastUpdatedAtUtc)).ToList(),
            Genres: mediaCache?.Genres,
            Description: mediaCache?.Description,
            AverageScore: mediaCache?.AverageScore,
            Popularity: mediaCache?.Popularity,
            Tags: mediaCache?.Tags?.Select(t => new AnimeTagDetail(t.Name, t.Rank, t.IsMediaSpoiler)).ToList(),
            SiteUrl: mediaCache?.SiteUrl,
            AiringStatus: mediaCache?.AiringStatus);
    }
}
