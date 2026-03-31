using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.GetWatchSpaceAnimeDetail;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using MediatR;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateSharedAnimeStatus;

public sealed class UpdateSharedAnimeStatusCommandHandler(
    IMembershipChecker membershipChecker,
    IAnimeTrackingRepository repository)
    : IRequestHandler<UpdateSharedAnimeStatusCommand, GetWatchSpaceAnimeDetailResult?>
{
    public async Task<GetWatchSpaceAnimeDetailResult?> Handle(
        UpdateSharedAnimeStatusCommand command,
        CancellationToken cancellationToken)
    {
        var isMember = await membershipChecker.IsMemberAsync(
            command.WatchSpaceId, command.RequestingUserId, cancellationToken);

        if (!isMember)
            throw new NotAWatchSpaceMemberException();

        var anime = await repository.GetByIdAsync(
            command.WatchSpaceId,
            WatchSpaceAnimeId.From(command.WatchSpaceAnimeId),
            cancellationToken);

        if (anime is null)
            return null;

        anime.UpdateSharedState(
            command.SharedStatus,
            command.SharedEpisodesWatched,
            command.Mood,
            command.Vibe,
            command.Pitch);

        await repository.SaveChangesAsync(cancellationToken);

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
