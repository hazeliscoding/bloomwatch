using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.UseCases.RenameWatchSpace;
using BloomWatch.Modules.WatchSpaces.Domain.Repositories;
using BloomWatch.Modules.WatchSpaces.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.InviteMember;

/// <summary>
/// Handles <see cref="InviteMemberCommand"/> by verifying the target user exists,
/// checking they are not already a member, creating an invitation on the aggregate,
/// and attempting to send the invitation email. Email send failures are non-fatal —
/// the invitation is persisted regardless, and <see cref="InviteMemberResult.EmailDeliveryFailed"/>
/// is set to <c>true</c> so the caller can surface a warning.
/// </summary>
/// <param name="repository">The watch space repository used for persistence.</param>
/// <param name="userReadModel">The read model used to resolve user identity by email.</param>
/// <param name="displayNameLookup">The read model used to resolve the inviter's display name.</param>
/// <param name="emailSender">The service responsible for delivering invitation emails.</param>
/// <param name="logger">Logger for recording email delivery failures.</param>
public sealed class InviteMemberCommandHandler(
    IWatchSpaceRepository repository,
    IUserReadModel userReadModel,
    IUserDisplayNameLookup displayNameLookup,
    IInvitationEmailSender emailSender,
    ILogger<InviteMemberCommandHandler> logger)
    : IRequestHandler<InviteMemberCommand, InviteMemberResult>
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(7);

    /// <summary>
    /// Invites a user to join a watch space by email. The invitation is valid for 7 days.
    /// If the email cannot be delivered after retries, the invitation is still created and
    /// <see cref="InviteMemberResult.EmailDeliveryFailed"/> is set to <c>true</c>.
    /// </summary>
    public async Task<InviteMemberResult> Handle(
        InviteMemberCommand command,
        CancellationToken cancellationToken)
    {
        var watchSpace = await repository.GetByIdWithMembersAsync(
            WatchSpaceId.From(command.WatchSpaceId), cancellationToken)
            ?? throw new WatchSpaceNotFoundException(command.WatchSpaceId);

        var user = await userReadModel.FindUserByEmailAsync(command.InvitedEmail, cancellationToken)
            ?? throw new InvitedUserNotFoundException(command.InvitedEmail);

        if (watchSpace.Members.Any(m => m.UserId == user.UserId))
            throw new AlreadyAMemberException(command.InvitedEmail);

        var expiresAt = DateTime.UtcNow.Add(DefaultExpiry);
        var invitation = watchSpace.InviteMember(command.InvitedEmail, command.RequestingUserId, expiresAt);

        await repository.SaveChangesAsync(cancellationToken);

        // Resolve inviter display name for the email; fall back gracefully if unavailable
        var names = await displayNameLookup.GetDisplayNamesAsync([command.RequestingUserId], cancellationToken);
        var inviterName = names.TryGetValue(command.RequestingUserId, out var n) ? n : "Someone";

        // Fire-and-forget: email delivery must not block or fail the HTTP response.
        // The invitation is already persisted; delivery failure is logged but non-fatal.
        _ = Task.Run(async () =>
        {
            try
            {
                await emailSender.SendAsync(
                    command.InvitedEmail,
                    invitation.Token,
                    watchSpace.Name,
                    inviterName,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to deliver invitation email to {Email} for watch space {WatchSpaceId}",
                    command.InvitedEmail, command.WatchSpaceId);
            }
        });

        return new InviteMemberResult(
            invitation.Id,
            invitation.InvitedEmail,
            invitation.Status.ToString(),
            invitation.ExpiresAtUtc,
            invitation.Token,
            false);
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
