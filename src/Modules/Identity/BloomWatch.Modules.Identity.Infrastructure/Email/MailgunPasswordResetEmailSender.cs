using System.Net.Http.Headers;
using System.Text;
using BloomWatch.Modules.Identity.Application.Abstractions;
using BloomWatch.Modules.Identity.Application.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BloomWatch.Modules.Identity.Infrastructure.Email;

/// <summary>
/// Sends password-reset emails via the Mailgun HTTP API (port 443 — works on all hosts).
/// </summary>
internal sealed class MailgunPasswordResetEmailSender(
    IConfiguration configuration,
    ILogger<MailgunPasswordResetEmailSender> logger) : IPasswordResetEmailSender
{
    public async Task SendAsync(
        string toEmail,
        string plainToken,
        CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["Email:Mailgun:ApiKey"]!;
        var domain = configuration["Email:Mailgun:Domain"]!;
        var fromAddress = configuration["Email:FromAddress"]!;
        var fromName = configuration["Email:FromName"] ?? "BloomWatch";
        var baseUrl = configuration["App:BaseUrl"] ?? "http://localhost:4200";

        var resetLink = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(plainToken)}";
        var (subject, htmlBody, plainBody) = PasswordResetEmailComposer.Compose(resetLink);

        using var client = new HttpClient();
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{apiKey}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("from", $"{fromName} <{fromAddress}>"),
            new KeyValuePair<string, string>("to", toEmail),
            new KeyValuePair<string, string>("subject", subject),
            new KeyValuePair<string, string>("html", htmlBody),
            new KeyValuePair<string, string>("text", plainBody),
        ]);

        var response = await client.PostAsync(
            $"https://api.mailgun.net/v3/{domain}/messages",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Mailgun rejected password-reset email to {Email}: {Status} {Body}",
                toEmail, response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        logger.LogInformation("Password-reset email sent to {Email} via Mailgun", toEmail);
    }
}
