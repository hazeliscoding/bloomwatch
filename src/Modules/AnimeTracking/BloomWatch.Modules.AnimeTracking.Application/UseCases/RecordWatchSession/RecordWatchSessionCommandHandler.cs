using BloomWatch.Modules.AnimeTracking.Application.Abstractions;
using BloomWatch.Modules.AnimeTracking.Domain.Exceptions;
using BloomWatch.Modules.AnimeTracking.Domain.Repositories;
using BloomWatch.Modules.AnimeTracking.Domain.ValueObjects;

namespace BloomWatch.Modules.AnimeTracking.Application.UseCases.RecordWatchSession;

public sealed class RecordWatchSessionCommandHandler(
    IMembershipChecker membershipChecker,
    IAnimeTrackingRepository repository)
{
    public async Task<RecordWatchSessionResult?> HandleAsync(
        RecordWatchSessionCommand command,
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

        var session = anime.RecordWatchSession(
            command.SessionDateUtc,
            command.StartEpisode,
            command.EndEpisode,
            command.Notes,
            command.RequestingUserId);

        await repository.SaveChangesAsync(cancellationToken);

        return new RecordWatchSessionResult(
            session.Id,
            session.SessionDateUtc,
            session.StartEpisode,
            session.EndEpisode,
            session.Notes,
            session.CreatedByUserId);
    }
}
