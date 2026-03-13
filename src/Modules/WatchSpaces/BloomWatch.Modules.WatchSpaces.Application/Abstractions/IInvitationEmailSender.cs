namespace BloomWatch.Modules.WatchSpaces.Application.Abstractions;

public interface IInvitationEmailSender
{
    Task SendAsync(string invitedEmail, string token, string watchSpaceName, CancellationToken cancellationToken = default);
}
