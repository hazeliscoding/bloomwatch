using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.TransferOwnership;

/// <summary>
/// Handles <see cref="TransferOwnershipCommand"/> by loading the watch space aggregate,
/// transferring ownership to the specified member, and persisting the change.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
public sealed class TransferOwnershipCommandHandler(IWatchSpaceRepository repository)
{
    /// <summary>
    /// Transfers ownership of a watch space from the current owner to another existing member.
    /// The current owner's role is demoted and the new owner's role is promoted.
    /// </summary>
    /// <param name="command">The command containing the watch space identifier, new owner, and requesting user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="WatchSpaceNotFoundException">Thrown when no watch space exists with the given identifier.</exception>
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
