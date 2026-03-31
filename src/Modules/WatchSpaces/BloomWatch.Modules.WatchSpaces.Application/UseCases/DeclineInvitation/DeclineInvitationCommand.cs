using BloomWatch.SharedKernel.CQRS;

namespace BloomWatch.Modules.WatchSpaces.Application.UseCases.DeclineInvitation;

/// <summary>
/// Command to decline a pending invitation to join a watch space.
/// </summary>
/// <param name="Token">The single-use invitation token received via email.</param>
/// <param name="DecliningUserEmail">The email address of the user declining the invitation, used to verify it matches the invitation.</param>
public sealed record DeclineInvitationCommand(string Token, string DecliningUserEmail) : ICommand;
