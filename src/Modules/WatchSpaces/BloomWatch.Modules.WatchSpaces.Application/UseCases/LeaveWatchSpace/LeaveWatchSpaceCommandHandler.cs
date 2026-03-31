using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using MediatR;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.LeaveWatchSpace;

/// <summary>
/// Handles <see cref="LeaveWatchSpaceCommand"/> by removing the requesting user from the watch space
/// and publishing a <c>MemberLeftWatchSpace</c> integration event.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
/// <param name="publisher">The integration event publisher used to notify other modules of the departure.</param>
public sealed class LeaveWatchSpaceCommandHandler(
    IWatchSpaceRepository repository,
    IIntegrationEventPublisher publisher)
    : IRequestHandler<LeaveWatchSpaceCommand>
{
    /// <summary>
    /// Removes the leaving user from the watch space membership and publishes an integration event.
    /// </summary>
    /// <param name="command">The command containing the watch space identifier and leaving user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="WatchSpaceNotFoundException">Thrown when no watch space exists with the given identifier.</exception>
    public async Task Handle(
        LeaveWatchSpaceCommand command,
        CancellationToken cancellationToken)
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
