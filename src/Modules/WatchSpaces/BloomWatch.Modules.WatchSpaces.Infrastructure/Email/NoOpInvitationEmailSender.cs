using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Email;

internal sealed class NoOpInvitationEmailSender(ILogger<NoOpInvitationEmailSender> logger) : IInvitationEmailSender
{
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
