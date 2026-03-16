using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Email;

/// <summary>
/// A no-op implementation of <see cref="IInvitationEmailSender"/> that logs invitation
/// details instead of delivering actual emails.
/// </summary>
/// <remarks>
/// This implementation is intended for local development and testing environments where
/// an email transport is unavailable. Replace with a real sender (e.g., SMTP, SendGrid)
/// for staging and production deployments.
/// </remarks>
/// <param name="logger">Logger used to record invitation details at Information level.</param>
internal sealed class NoOpInvitationEmailSender(ILogger<NoOpInvitationEmailSender> logger) : IInvitationEmailSender
{
    /// <summary>
    /// Logs the invitation details without sending an actual email.
    /// </summary>
    /// <param name="invitedEmail">The email address of the invitee.</param>
    /// <param name="token">The unique invitation token the invitee uses to accept.</param>
    /// <param name="watchSpaceName">The display name of the watch space the invitee is being invited to.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation (unused in this implementation).</param>
    /// <returns>A completed task, since no I/O is performed.</returns>
    public Task SendAsync(
        string invitedEmail,
        string token,
        string watchSpaceName,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "NOOP: Invitation email to {Email} for watch space '{WatchSpaceName}'. Token: {Token}",
            invitedEmail, watchSpaceName, token);

        return Task.CompletedTask;
    }
}
