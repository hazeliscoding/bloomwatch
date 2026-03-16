using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RemoveMember;

/// <summary>
/// Handles <see cref="RemoveMemberCommand"/> by loading the watch space aggregate,
/// removing the target member, and publishing a <c>MemberLeftWatchSpace</c> integration event.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
/// <param name="publisher">The integration event publisher used to notify other modules of the removal.</param>
public sealed class RemoveMemberCommandHandler(
    IWatchSpaceRepository repository,
    IIntegrationEventPublisher publisher)
{
    /// <summary>
    /// Removes a member from the watch space and publishes an integration event to notify other modules.
    /// </summary>
    /// <param name="command">The command containing the watch space identifier, target member, and requesting user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="WatchSpaceNotFoundException">Thrown when no watch space exists with the given identifier.</exception>
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
