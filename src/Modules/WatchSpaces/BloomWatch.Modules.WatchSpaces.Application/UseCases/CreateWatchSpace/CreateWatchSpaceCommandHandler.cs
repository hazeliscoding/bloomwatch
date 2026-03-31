using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using MediatR;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.CreateWatchSpace;

/// <summary>
/// Handles <see cref="CreateWatchSpaceCommand"/> by creating a new watch space aggregate
/// and persisting it to the repository.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
/// <param name="displayNameLookup">Resolves user IDs to display names for member previews.</param>
public sealed class CreateWatchSpaceCommandHandler(
    IWatchSpaceRepository repository,
    IUserDisplayNameLookup displayNameLookup)
    : IRequestHandler<CreateWatchSpaceCommand, CreateWatchSpaceResult>
{
    /// <summary>
    /// Creates a new watch space and assigns the requesting user as its owner.
    /// </summary>
    /// <param name="command">The command containing the watch space name and requesting user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="CreateWatchSpaceResult"/> containing the new watch space details.</returns>
    public async Task<CreateWatchSpaceResult> Handle(
        CreateWatchSpaceCommand command,
        CancellationToken cancellationToken)
    {
        var watchSpace = WatchSpace.Create(command.Name, command.RequestingUserId);

        await repository.AddAsync(watchSpace, cancellationToken);

        var displayNames = await displayNameLookup.GetDisplayNamesAsync(
            [command.RequestingUserId], cancellationToken);

        var previews = new List<CreateWatchSpaceMemberPreview>
        {
            new(displayNames.GetValueOrDefault(command.RequestingUserId, "Unknown"))
        };

        return new CreateWatchSpaceResult(
            watchSpace.Id.Value,
            watchSpace.Name,
            watchSpace.CreatedAtUtc,
            "Owner",
            watchSpace.Members.Count,
            previews);
    }
}
