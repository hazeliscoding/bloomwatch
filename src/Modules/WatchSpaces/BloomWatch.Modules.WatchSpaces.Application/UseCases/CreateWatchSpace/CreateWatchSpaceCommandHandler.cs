using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.CreateWatchSpace;

/// <summary>
/// Handles <see cref="CreateWatchSpaceCommand"/> by creating a new watch space aggregate
/// and persisting it to the repository.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
public sealed class CreateWatchSpaceCommandHandler(IWatchSpaceRepository repository)
{
    /// <summary>
    /// Creates a new watch space and assigns the requesting user as its owner.
    /// </summary>
    /// <param name="command">The command containing the watch space name and requesting user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="CreateWatchSpaceResult"/> containing the new watch space details.</returns>
    public async Task<CreateWatchSpaceResult> HandleAsync(
        CreateWatchSpaceCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = WatchSpace.Create(command.Name, command.RequestingUserId);

        await repository.AddAsync(watchSpace, cancellationToken);

        return new CreateWatchSpaceResult(
            watchSpace.Id.Value,
            watchSpace.Name,
            watchSpace.CreatedAtUtc,
            "Owner");
    }
}
