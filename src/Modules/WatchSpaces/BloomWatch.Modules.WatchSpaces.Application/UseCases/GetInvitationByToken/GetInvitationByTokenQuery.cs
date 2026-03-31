using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.GetInvitationByToken;

/// <summary>
/// Query to retrieve an invitation preview by its token. Used by the invitee to view
/// the invitation details before accepting or declining.
/// </summary>
/// <param name="Token">The unique invitation token.</param>
/// <param name="RequestingUserEmail">The email of the authenticated user making the request.</param>
public sealed record GetInvitationByTokenQuery(string Token, string RequestingUserEmail) : IQuery<InvitationPreviewResult>;

/// <summary>
/// Preview of an invitation returned to the invitee.
/// </summary>
/// <param name="WatchSpaceId">The identifier of the watch space the invitation belongs to.</param>
/// <param name="WatchSpaceName">The name of the watch space.</param>
/// <param name="InvitedEmail">The email address the invitation was sent to.</param>
/// <param name="Status">The current status of the invitation.</param>
/// <param name="ExpiresAt">The UTC timestamp when the invitation expires.</param>
public sealed record InvitationPreviewResult(
    Guid WatchSpaceId,
    string WatchSpaceName,
    string InvitedEmail,
    string Status,
    DateTime ExpiresAt);
