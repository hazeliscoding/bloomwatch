namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.ListInvitations;

/// <summary>
/// Query to list all invitations for a watch space. Only the owner can view invitations.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space whose invitations to list.</param>
/// <param name="RequestingUserId">The identifier of the user making the request (must be the owner).</param>
public sealed record ListInvitationsQuery(Guid WatchSpaceId, Guid RequestingUserId);

/// <summary>
/// A projection of a single invitation associated with a watch space.
/// </summary>
/// <param name="InvitationId">The unique identifier of the invitation.</param>
/// <param name="InvitedEmail">The email address the invitation was sent to.</param>
/// <param name="Status">The current status of the invitation (e.g., <c>"Pending"</c>, <c>"Accepted"</c>, <c>"Declined"</c>, <c>"Revoked"</c>).</param>
/// <param name="ExpiresAt">The UTC timestamp when the invitation expires.</param>
/// <param name="CreatedAt">The UTC timestamp when the invitation was created.</param>
public sealed record InvitationDetail(
    Guid InvitationId,
    string InvitedEmail,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);
