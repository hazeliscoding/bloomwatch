using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.InviteMember;

/// <summary>
/// Command to invite a user to join a watch space by their email address.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space to invite the user to.</param>
/// <param name="InvitedEmail">The email address of the user being invited.</param>
/// <param name="RequestingUserId">The identifier of the user issuing the invitation (must be the owner).</param>
public sealed record InviteMemberCommand(Guid WatchSpaceId, string InvitedEmail, Guid RequestingUserId) : ICommand<InviteMemberResult>;

/// <summary>
/// The result returned after an invitation is successfully created and sent.
/// </summary>
/// <param name="InvitationId">The unique identifier of the created invitation.</param>
/// <param name="InvitedEmail">The email address the invitation was sent to.</param>
/// <param name="Status">The current status of the invitation (e.g., <c>"Pending"</c>).</param>
/// <param name="ExpiresAt">The UTC timestamp when the invitation expires.</param>
/// <param name="Token">The single-use token the invitee uses to accept the invitation.</param>
/// <param name="EmailDeliveryFailed">
/// <c>true</c> if all email delivery attempts failed after retries; the invitation is still valid.
/// The owner should notify the invitee manually if this is <c>true</c>.
/// </param>
public sealed record InviteMemberResult(
    Guid InvitationId,
    string InvitedEmail,
    string Status,
    DateTime ExpiresAt,
    string Token,
    bool EmailDeliveryFailed);
