using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;

/// <summary>
/// Handles <see cref="RenameWatchSpaceCommand"/> by loading the watch space aggregate,
/// applying the rename, and persisting the change.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
public sealed class RenameWatchSpaceCommandHandler(IWatchSpaceRepository repository)
{
    /// <summary>
    /// Renames a watch space to the specified new name.
    /// </summary>
    /// <param name="command">The command containing the watch space identifier, new name, and requesting user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="RenameWatchSpaceResult"/> containing the updated watch space identifier and name.</returns>
    /// <exception cref="WatchSpaceNotFoundException">Thrown when no watch space exists with the given identifier.</exception>
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

/// <summary>
/// The result returned after a watch space is successfully renamed.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the renamed watch space.</param>
/// <param name="Name">The new display name of the watch space.</param>
public sealed record RenameWatchSpaceResult(Guid WatchSpaceId, string Name);

/// <summary>
/// Thrown when a watch space cannot be found by its identifier.
/// </summary>
/// <param name="id">The identifier that was not found.</param>
public sealed class WatchSpaceNotFoundException(Guid id)
    : Exception($"Watch space '{id}' not found.");
