using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.UpdateParticipantProgress;

public sealed class UpdateParticipantProgressCommandHandler(
    IMembershipChecker membershipChecker,
    IAnimeTrackingRepository repository)
{
    public async Task<UpdateParticipantProgressResult?> HandleAsync(
        UpdateParticipantProgressCommand command,
        CancellationToken cancellationToken = default)
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

        var entry = anime.UpdateParticipantProgress(
            command.RequestingUserId,
            command.IndividualStatus,
            command.EpisodesWatched);

        await repository.SaveChangesAsync(cancellationToken);

        return new UpdateParticipantProgressResult(
            entry.UserId,
            entry.IndividualStatus.ToString(),
            entry.EpisodesWatched,
            entry.RatingScore,
            entry.RatingNotes,
            entry.LastUpdatedAtUtc);
    }
}
