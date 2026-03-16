namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.AcceptInvitation;

/// <summary>
/// Command to accept a pending invitation to join a watch space.
/// </summary>
/// <param name="Token">The single-use invitation token received via email.</param>
/// <param name="AcceptingUserId">The identifier of the user accepting the invitation.</param>
/// <param name="AcceptingUserEmail">The email address of the accepting user, used to verify it matches the invitation.</param>
public sealed record AcceptInvitationCommand(string Token, Guid AcceptingUserId, string AcceptingUserEmail);
