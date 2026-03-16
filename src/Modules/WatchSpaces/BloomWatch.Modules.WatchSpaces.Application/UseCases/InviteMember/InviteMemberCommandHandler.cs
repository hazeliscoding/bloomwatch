using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.InviteMember;

/// <summary>
/// Handles <see cref="InviteMemberCommand"/> by verifying the target user exists,
/// checking they are not already a member, creating an invitation on the aggregate,
/// and sending the invitation email.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
/// <param name="userReadModel">The read model used to resolve user identity by email.</param>
/// <param name="emailSender">The service responsible for delivering invitation emails.</param>
public sealed class InviteMemberCommandHandler(
    IWatchSpaceRepository repository,
    IUserReadModel userReadModel,
    IInvitationEmailSender emailSender)
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(7);

    /// <summary>
    /// Invites a user to join a watch space by email. The invitation is valid for 7 days.
    /// </summary>
    /// <param name="command">The command containing the watch space identifier, invitee email, and requesting user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="InviteMemberResult"/> with the invitation details and acceptance token.</returns>
    /// <exception cref="WatchSpaceNotFoundException">Thrown when no watch space exists with the given identifier.</exception>
    /// <exception cref="InvitedUserNotFoundException">Thrown when no registered user matches the invited email address.</exception>
    /// <exception cref="AlreadyAMemberException">Thrown when the invited user is already a member of the watch space.</exception>
    public async Task<InviteMemberResult> HandleAsync(
        InviteMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(command.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(command.WatchSpaceId);

        var user = await userReadModel.FindUserByEmailAsync(command.InvitedEmail, cancellationToken)
            ?? throw new InvitedUserNotFoundException(command.InvitedEmail);

        // Check if already a member
        if (watchSpace.Members.Any(m => m.UserId == user.UserId))
            throw new AlreadyAMemberException(command.InvitedEmail);

        var expiresAt = DateTime.UtcNow.Add(DefaultExpiry);
        var invitation = watchSpace.InviteMember(command.InvitedEmail, command.RequestingUserId, expiresAt);

        await repository.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(
            command.InvitedEmail,
            invitation.Token,
            watchSpace.Name,
            cancellationToken);

        return new InviteMemberResult(
            invitation.Id,
            invitation.InvitedEmail,
            invitation.Status.ToString(),
            invitation.ExpiresAtUtc,
            invitation.Token);
    }
}

/// <summary>
/// Thrown when the invited email address does not match any registered user.
/// </summary>
/// <param name="email">The email address that was not found.</param>
public sealed class InvitedUserNotFoundException(string email)
    : Exception($"No registered user found with email '{email}'.");

/// <summary>
/// Thrown when attempting to invite a user who is already a member of the watch space.
/// </summary>
/// <param name="email">The email address of the user who is already a member.</param>
public sealed class AlreadyAMemberException(string email)
    : Exception($"User with email '{email}' is already a member of this watch space.");
