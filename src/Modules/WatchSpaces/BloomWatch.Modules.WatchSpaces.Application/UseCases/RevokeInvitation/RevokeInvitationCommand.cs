using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.RevokeInvitation;

/// <summary>
/// Command to revoke a pending invitation, preventing the invitee from accepting it.
/// Only the watch space owner can revoke invitations.
/// </summary>
/// <param name="WatchSpaceId">The unique identifier of the watch space the invitation belongs to.</param>
/// <param name="InvitationId">The unique identifier of the invitation to revoke.</param>
/// <param name="RequestingUserId">The identifier of the user requesting the revocation (must be the owner).</param>
public sealed record RevokeInvitationCommand(Guid WatchSpaceId, Guid InvitationId, Guid RequestingUserId) : ICommand;
