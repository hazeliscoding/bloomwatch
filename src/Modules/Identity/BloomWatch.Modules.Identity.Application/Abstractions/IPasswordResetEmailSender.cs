namespace BloomWatch.Modules.Identity.Application.Abstractions;

/// <summary>
/// Sends password-reset emails to users who have requested a reset link.
/// </summary>
public interface IPasswordResetEmailSender
{
    /// <summary>
    /// Sends a password-reset email containing a secure, time-limited link.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="plainToken">The plain (un-hashed) reset token to embed in the link.</param>
    /// <param name="cancellationToken">A token to cancel the send operation.</param>
    Task SendAsync(string toEmail, string plainToken, CancellationToken cancellationToken = default);
}
