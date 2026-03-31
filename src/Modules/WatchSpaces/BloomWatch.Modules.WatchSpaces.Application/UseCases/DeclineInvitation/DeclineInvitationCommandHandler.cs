using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using MediatR;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.DeclineInvitation;

/// <summary>
/// Handles <see cref="DeclineInvitationCommand"/> by resolving the watch space from the invitation token
/// and marking the invitation as declined.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
public sealed class DeclineInvitationCommandHandler(IWatchSpaceRepository repository)
    : IRequestHandler<DeclineInvitationCommand>
{
    /// <summary>
    /// Declines a pending invitation. The invitation is marked as declined and can no longer be accepted.
    /// </summary>
    /// <param name="command">The command containing the invitation token and declining user's email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvitationNotFoundException">Thrown when no invitation matches the provided token.</exception>
    public async Task Handle(
        DeclineInvitationCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var watchSpace = await repository.GetByInvitationTokenAsync(command.Token, cancellationToken)
            ?? throw new InvitationNotFoundException();

        watchSpace.DeclineInvitation(command.Token, command.DecliningUserEmail, now);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
