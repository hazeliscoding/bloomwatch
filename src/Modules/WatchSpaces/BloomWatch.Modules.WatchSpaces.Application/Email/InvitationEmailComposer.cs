namespace BloomWatch.Modules.WatchSpaces.Application.Email;

/// <summary>
/// Composes invitation email content (subject, HTML body, plain-text body).
/// </summary>
public static class InvitationEmailComposer
{
    /// <summary>
    /// Builds the subject line, kawaii-branded HTML body, and plain-text fallback for an invitation email.
    /// </summary>
    /// <param name="inviterName">Display name of the user sending the invitation.</param>
    /// <param name="spaceName">Name of the watch space the recipient is being invited to.</param>
    /// <param name="token">Unique invitation token used to accept or decline.</param>
    /// <param name="baseUrl">Frontend base URL (e.g. https://bloomwatch.app or http://localhost:4200).</param>
    /// <returns>A tuple of (subject, htmlBody, plainTextBody).</returns>
    public static (string Subject, string HtmlBody, string PlainTextBody) Compose(
        string inviterName,
        string spaceName,
        string token,
        string baseUrl)
    {
        var acceptUrl = $"{baseUrl.TrimEnd('/')}/invitations/{token}/accept";
        var declineUrl = $"{baseUrl.TrimEnd('/')}/invitations/{token}/decline";

        var subject = $"{inviterName} invited you to join {spaceName} on BloomWatch";

        var html = $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width,initial-scale=1">
              <title>{subject}</title>
            </head>
            <body style="margin:0;padding:0;background-color:#FDF0F7;font-family:'Nunito','Helvetica Neue',Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#FDF0F7;padding:32px 16px;">
                <tr>
                  <td align="center">
                    <table width="560" cellpadding="0" cellspacing="0" style="background-color:#FFFFFF;border-radius:20px;overflow:hidden;box-shadow:0 4px 20px rgba(255,107,157,0.15);max-width:560px;">
                      <!-- Header -->
                      <tr>
                        <td style="background:linear-gradient(135deg,#FF6B9D,#C084FC);padding:32px;text-align:center;">
                          <div style="font-size:28px;font-weight:900;color:#FFFFFF;letter-spacing:-0.5px;">🌸 BloomWatch</div>
                          <div style="font-size:14px;color:rgba(255,255,255,0.85);margin-top:4px;">your shared anime tracker</div>
                        </td>
                      </tr>
                      <!-- Body -->
                      <tr>
                        <td style="padding:36px 40px 28px;">
                          <p style="margin:0 0 8px;font-size:22px;font-weight:800;color:#1A1A2E;">You're invited! 🎉</p>
                          <p style="margin:0 0 24px;font-size:16px;color:#4A4A6A;line-height:1.6;">
                            <strong style="color:#FF6B9D;">{inviterName}</strong> has invited you to join
                            <strong style="color:#C084FC;">{spaceName}</strong> on BloomWatch.
                          </p>
                          <p style="margin:0 0 28px;font-size:15px;color:#6B6B8A;line-height:1.6;">
                            Track anime together, share ratings, and discover how compatible your tastes really are 🌟
                          </p>
                          <!-- Accept button -->
                          <table cellpadding="0" cellspacing="0" style="margin-bottom:16px;">
                            <tr>
                              <td style="background-color:#FF6B9D;border-radius:12px;padding:14px 32px;">
                                <a href="{acceptUrl}" style="color:#FFFFFF;font-size:16px;font-weight:800;text-decoration:none;letter-spacing:0.3px;">✨ Accept Invitation</a>
                              </td>
                            </tr>
                          </table>
                          <!-- Decline link -->
                          <p style="margin:0;font-size:13px;color:#9999BB;">
                            Not interested? <a href="{declineUrl}" style="color:#C084FC;text-decoration:underline;">Decline invitation</a>
                          </p>
                        </td>
                      </tr>
                      <!-- Footer -->
                      <tr>
                        <td style="background-color:#FDF0F7;padding:20px 40px;text-align:center;border-top:1px solid #F0D6E8;">
                          <p style="margin:0;font-size:12px;color:#AAAACC;">This invitation expires in 7 days · Sent via BloomWatch</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

        var plain = $"""
            You're invited to BloomWatch!

            {inviterName} has invited you to join "{spaceName}" on BloomWatch — your shared anime tracker.

            Accept invitation:
            {acceptUrl}

            Decline invitation:
            {declineUrl}

            This invitation expires in 7 days.
            """;

        return (subject, html, plain);
    }
}
