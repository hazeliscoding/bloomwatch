using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Application.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Polly;
using Polly.Retry;

namespace BloomWatch.Modules.Identity.Infrastructure.Email;

/// <summary>
/// Sends password-reset emails via SMTP using MailKit with Polly retry (3 total attempts).
/// </summary>
internal sealed class SmtpPasswordResetEmailSender(
    IConfiguration configuration,
    ILogger<SmtpPasswordResetEmailSender> logger) : IPasswordResetEmailSender
{
    private static readonly AsyncRetryPolicy RetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(retryCount: 2, sleepDurationProvider: _ => TimeSpan.FromSeconds(1));

    public async Task SendAsync(string toEmail, string plainToken, CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:Smtp:Host"]!;
        var port = int.Parse(configuration["Email:Smtp:Port"] ?? "587");
        var username = configuration["Email:Smtp:Username"] ?? string.Empty;
        var password = configuration["Email:Smtp:Password"] ?? string.Empty;
        var fromAddress = configuration["Email:FromAddress"]!;
        var fromName = configuration["Email:FromName"] ?? "BloomWatch";
        var baseUrl = configuration["App:BaseUrl"] ?? "http://localhost:4200";

        var resetLink = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(plainToken)}";
        var (subject, htmlBody, plainBody) = PasswordResetEmailComposer.Compose(resetLink);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody, TextBody = plainBody };
        message.Body = bodyBuilder.ToMessageBody();

        await RetryPolicy.ExecuteAsync(async () =>
        {
            using var client = new SmtpClient();
            var secureSocketOptions = string.IsNullOrEmpty(username)
                ? SecureSocketOptions.None
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(host, port, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(username))
                await client.AuthenticateAsync(username, password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);
        });

        logger.LogInformation("Password-reset email sent to {Email}", toEmail);
    }
}
