using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;
using MediatR;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantRating;

public sealed class UpdateParticipantRatingCommandHandler(
    IMembershipChecker membershipChecker,
    IAnimeTrackingRepository repository)
    : IRequestHandler<UpdateParticipantRatingCommand, ParticipantDetailResult?>
{
    public async Task<ParticipantDetailResult?> Handle(
        UpdateParticipantRatingCommand command,
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

        var entry = anime.UpdateParticipantRating(
            command.RequestingUserId,
            command.RatingScore,
            command.RatingNotes,
            command.UpdateNotes);

        await repository.SaveChangesAsync(cancellationToken);

        return new ParticipantDetailResult(
            entry.UserId,
            entry.IndividualStatus.ToString(),
            entry.EpisodesWatched,
            entry.RatingScore,
            entry.RatingNotes,
            entry.LastUpdatedAtUtc);
    }
}
