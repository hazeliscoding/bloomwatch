using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RemoveMember;

public sealed class RemoveMemberCommandHandler(
    IWatchSpaceRepository repository,
    IIntegrationEventPublisher publisher)
{
    public async Task HandleAsync(
        RemoveMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(command.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(command.WatchSpaceId);

        watchSpace.RemoveMember(command.TargetUserId, command.RequestingUserId);
        await repository.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(new MemberLeftWatchSpace(
            command.WatchSpaceId,
            command.TargetUserId,
            DateTime.UtcNow), cancellationToken);
    }
}
