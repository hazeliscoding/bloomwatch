using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.TransferOwnership;

public sealed class TransferOwnershipCommandHandler(IWatchSpaceRepository repository)
{
    public async Task HandleAsync(
        TransferOwnershipCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(command.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(command.WatchSpaceId);

        watchSpace.TransferOwnership(command.NewOwnerId, command.RequestingUserId);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
