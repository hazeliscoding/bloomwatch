using System.Net.Http.Headers;
using System.Text;
using BloomWatch.Modules.WatchSpaces.Application.Abstractions;
using BloomWatch.Modules.WatchSpaces.Application.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BloomWatch.Modules.WatchSpaces.Infrastructure.Email;

/// <summary>
/// Sends invitation emails via the Mailgun HTTP API (port 443 — works on all hosts).
/// </summary>
internal sealed class MailgunInvitationEmailSender(
    IConfiguration configuration,
    ILogger<MailgunInvitationEmailSender> logger) : IInvitationEmailSender
{
    public async Task SendAsync(
        string invitedEmail,
        string token,
        string watchSpaceName,
        string inviterName,
        CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["Email:Mailgun:ApiKey"]!;
        var domain = configuration["Email:Mailgun:Domain"]!;
        var fromAddress = configuration["Email:FromAddress"]!;
        var fromName = configuration["Email:FromName"] ?? "BloomWatch";
        var baseUrl = configuration["App:BaseUrl"] ?? "http://localhost:4200";

        var (subject, htmlBody, plainBody) = InvitationEmailComposer.Compose(
            inviterName, watchSpaceName, token, baseUrl);

        using var client = new HttpClient();
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{apiKey}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("from", $"{fromName} <{fromAddress}>"),
            new KeyValuePair<string, string>("to", invitedEmail),
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
            logger.LogError("Mailgun rejected invitation email to {Email}: {Status} {Body}",
                invitedEmail, response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        logger.LogInformation("Invitation email sent to {Email} via Mailgun", invitedEmail);
    }
}
