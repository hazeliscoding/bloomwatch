using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.LeaveWatchSpace;

public sealed class LeaveWatchSpaceCommandHandler(
    IWatchSpaceRepository repository,
    IIntegrationEventPublisher publisher)
{
    public async Task HandleAsync(
        LeaveWatchSpaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(command.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(command.WatchSpaceId);

        watchSpace.Leave(command.LeavingUserId);
        await repository.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(new MemberLeftWatchSpace(
            command.WatchSpaceId,
            command.LeavingUserId,
            DateTime.UtcNow), cancellationToken);
    }
}
