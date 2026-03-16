namespace BloomWatch.Modules.WatchSpaces.Application.Abstractions;

/// <summary>
/// Sends invitation emails to users who have been invited to join a watch space.
/// </summary>
/// <remarks>
/// Implementations handle email template rendering, delivery, and any retry logic.
/// The token included in the email allows the recipient to accept the invitation.
/// </remarks>
public interface IInvitationEmailSender
{
    /// <summary>
    /// Sends an invitation email to the specified address.
    /// </summary>
    /// <param name="invitedEmail">The email address of the invited user.</param>
    /// <param name="token">A unique, single-use token the recipient uses to accept the invitation.</param>
    /// <param name="watchSpaceName">The display name of the watch space the user is being invited to.</param>
    /// <param name="cancellationToken">A token to cancel the send operation.</param>
    /// <returns>A task that completes when the email has been submitted for delivery.</returns>
    Task SendAsync(string invitedEmail, string token, string watchSpaceName, CancellationToken cancellationToken = default);
}
