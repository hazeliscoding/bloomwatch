using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;

public sealed class RenameWatchSpaceCommandHandler(IWatchSpaceRepository repository)
{
    public async Task<RenameWatchSpaceResult> HandleAsync(
        RenameWatchSpaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(command.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(command.WatchSpaceId);

        watchSpace.Rename(command.NewName, command.RequestingUserId);
        await repository.SaveChangesAsync(cancellationToken);

        return new RenameWatchSpaceResult(watchSpace.Id.Value, watchSpace.Name);
    }
}

public sealed record RenameWatchSpaceResult(Guid WatchSpaceId, string Name);

public sealed class WatchSpaceNotFoundException(Guid id)
    : Exception($"Watch space '{id}' not found.");
