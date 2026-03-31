using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly;
using Polly.Retry;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Email;

/// <summary>
/// Sends invitation emails via SMTP using MailKit. Retries up to 2 additional times
/// (3 total attempts) with a 1-second delay on transient failure.
/// </summary>
internal sealed class SmtpInvitationEmailSender(
    IConfiguration configuration,
    ILogger<SmtpInvitationEmailSender> logger) : IInvitationEmailSender
{
    private static readonly AsyncRetryPolicy RetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(retryCount: 2, sleepDurationProvider: _ => TimeSpan.FromSeconds(1));

    public async Task SendAsync(
        string invitedEmail,
        string token,
        string watchSpaceName,
        string inviterName,
        CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:Smtp:Host"]!;
        var port = int.Parse(configuration["Email:Smtp:Port"] ?? "587");
        var username = configuration["Email:Smtp:Username"] ?? string.Empty;
        var password = configuration["Email:Smtp:Password"] ?? string.Empty;
        var fromAddress = configuration["Email:FromAddress"]!;
        var fromName = configuration["Email:FromName"] ?? "BloomWatch";
        var baseUrl = configuration["App:BaseUrl"] ?? "http://localhost:4200";

        var (subject, htmlBody, plainBody) = InvitationEmailComposer.Compose(
            inviterName, watchSpaceName, token, baseUrl);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(invitedEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = plainBody,
        };
        message.Body = bodyBuilder.ToMessageBody();

        await RetryPolicy.ExecuteAsync(async () =>
        {
            using var client = new SmtpClient();
            var secureSocketOptions = string.IsNullOrEmpty(username)
                ? SecureSocketOptions.None   // Mailpit: no TLS, no auth
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(host, port, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(username))
                await client.AuthenticateAsync(username, password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);
        });

        logger.LogInformation(
            "Invitation email sent to {Email} for watch space '{SpaceName}'",
            invitedEmail, watchSpaceName);
    }
}
