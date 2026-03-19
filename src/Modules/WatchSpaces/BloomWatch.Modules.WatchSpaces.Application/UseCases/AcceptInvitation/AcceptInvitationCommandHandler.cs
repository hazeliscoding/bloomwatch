using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Contracts.IntegrationEvents;
using BloomWatch.Modules.WatchSpaces.Domain.Aggregates;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.AcceptInvitation;

/// <summary>
/// Handles <see cref="AcceptInvitationCommand"/> by resolving the watch space from the invitation token,
/// accepting the invitation on the aggregate, and publishing a <c>MemberJoinedWatchSpace</c> integration event.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
/// <param name="publisher">The integration event publisher used to notify other modules of the new membership.</param>
public sealed class AcceptInvitationCommandHandler(
    IWatchSpaceRepository repository,
    IIntegrationEventPublisher publisher)
{
    /// <summary>
    /// Accepts a pending invitation, adds the user as a member, and publishes a membership event.
    /// </summary>
    /// <param name="command">The command containing the invitation token, accepting user identifier, and email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result containing the watch space identifier.</returns>
    /// <exception cref="InvitationNotFoundException">Thrown when no invitation matches the provided token.</exception>
    public async Task<AcceptInvitationResult> HandleAsync(
        AcceptInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var watchSpace = await repository.GetByInvitationTokenAsync(command.Token, cancellationToken)
            ?? throw new Domain.Aggregates.InvitationNotFoundException();

        var member = watchSpace.AcceptInvitation(
            command.Token,
            command.AcceptingUserId,
            command.AcceptingUserEmail,
            now);

        await repository.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(new MemberJoinedWatchSpace(
            watchSpace.Id.Value,
            member.UserId,
            member.Role.ToString(),
            member.JoinedAtUtc), cancellationToken);

        return new AcceptInvitationResult(watchSpace.Id.Value);
    }
}
