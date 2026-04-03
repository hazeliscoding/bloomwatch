namespace BloomWatch.Modules.Identity.Application.Email;

/// <summary>
/// Composes password-reset email content (subject, HTML body, plain-text body).
/// </summary>
public static class PasswordResetEmailComposer
{
    /// <summary>
    /// Builds the subject line, kawaii-branded HTML body, and plain-text fallback for a password-reset email.
    /// </summary>
    /// <param name="resetLink">The full reset URL including the token query parameter.</param>
    /// <returns>A tuple of (subject, htmlBody, plainTextBody).</returns>
    public static (string Subject, string HtmlBody, string PlainTextBody) Compose(string resetLink)
    {
        const string subject = "Reset your BloomWatch password";

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
                          <p style="margin:0 0 8px;font-size:22px;font-weight:800;color:#1A1A2E;">Password reset 🔑</p>
                          <p style="margin:0 0 24px;font-size:16px;color:#4A4A6A;line-height:1.6;">
                            We received a request to reset your BloomWatch password.
                            Click the button below to choose a new one.
                          </p>
                          <p style="margin:0 0 28px;font-size:15px;color:#6B6B8A;line-height:1.6;">
                            This link is valid for <strong>1 hour</strong> and can only be used once. 🌟
                          </p>
                          <!-- Reset button -->
                          <table cellpadding="0" cellspacing="0" style="margin-bottom:20px;">
                            <tr>
                              <td style="background-color:#FF6B9D;border-radius:12px;padding:14px 32px;">
                                <a href="{resetLink}" style="color:#FFFFFF;font-size:16px;font-weight:800;text-decoration:none;letter-spacing:0.3px;">✨ Reset my password</a>
                              </td>
                            </tr>
                          </table>
                          <!-- Fallback link -->
                          <p style="margin:0 0 8px;font-size:13px;color:#9999BB;">
                            Button not working? Copy and paste this link into your browser:
                          </p>
                          <p style="margin:0;font-size:12px;color:#C084FC;word-break:break-all;">
                            <a href="{resetLink}" style="color:#C084FC;text-decoration:underline;">{resetLink}</a>
                          </p>
                        </td>
                      </tr>
                      <!-- Footer -->
                      <tr>
                        <td style="background-color:#FDF0F7;padding:20px 40px;text-align:center;border-top:1px solid #F0D6E8;">
                          <p style="margin:0;font-size:12px;color:#AAAACC;">
                            Didn't request this? You can safely ignore this email · Sent via BloomWatch
                          </p>
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
            Password reset — BloomWatch

            We received a request to reset your BloomWatch password.
            Use the link below to set a new password (valid for 1 hour, single use):

            {resetLink}

            Didn't request this? You can safely ignore this email.
            """;

        return (subject, html, plain);
    }
}
