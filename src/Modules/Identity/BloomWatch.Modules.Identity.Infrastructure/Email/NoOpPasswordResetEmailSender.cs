using BloomWatch.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace BloomWatch.Modules.Identity.Infrastructure.Email;

/// <summary>
/// A no-op <see cref="IPasswordResetEmailSender"/> used when SMTP is not configured (e.g., tests).
/// Logs the reset link at Debug level instead of sending an email.
/// </summary>
internal sealed class NoOpPasswordResetEmailSender(ILogger<NoOpPasswordResetEmailSender> logger)
    : IPasswordResetEmailSender
{
    public Task SendAsync(string toEmail, string plainToken, CancellationToken cancellationToken = default)
    {
        logger.LogDebug(
            "[NoOp] Password-reset email suppressed. Recipient: {Email}, Token: {Token}",
            toEmail, plainToken);
        return Task.CompletedTask;
    }
}
