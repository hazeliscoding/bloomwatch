using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RevokeInvitation;

/// <summary>
/// Handles <see cref="RevokeInvitationCommand"/> by loading the watch space aggregate,
/// revoking the specified invitation, and persisting the change.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
public sealed class RevokeInvitationCommandHandler(IWatchSpaceRepository repository)
{
    /// <summary>
    /// Revokes a pending invitation so it can no longer be accepted by the invitee.
    /// </summary>
    /// <param name="command">The command containing the watch space identifier, invitation identifier, and requesting user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="WatchSpaceNotFoundException">Thrown when no watch space exists with the given identifier.</exception>
    public async Task HandleAsync(
        RevokeInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(command.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(command.WatchSpaceId);

        watchSpace.RevokeInvitation(command.InvitationId, command.RequestingUserId);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
