## ADDED Requirements

### Requirement: Invitation email is composed with inviter name, space name, action links, and kawaii branding
The system SHALL provide an `InvitationEmailComposer` that produces both an HTML and a plain-text body given: inviter display name, watch space name, invitee email, invitation token, and the configured `App:BaseUrl`. The HTML body SHALL use inline CSS matching the BloomWatch kawaii brand: hot-pink/lilac color palette (`#FF6B9D`, `#C084FC`), Nunito/sans-serif font stack, rounded corners, and a branded header with the BloomWatch name. The email SHALL include a styled accept button and a plain decline link. The email SHALL include a link to accept (`{baseUrl}/invitations/{token}/accept`) and a link to decline (`{baseUrl}/invitations/{token}/decline`).

#### Scenario: Links use configured base URL
- **WHEN** `App:BaseUrl` is `https://bloomwatch.app` and the invitation token is `abc123`
- **THEN** the accept link SHALL be `https://bloomwatch.app/invitations/abc123/accept` and the decline link SHALL be `https://bloomwatch.app/invitations/abc123/decline`

#### Scenario: Subject line identifies the inviting user and space
- **WHEN** the email is composed for inviter "Hazel" and space "Anime Club"
- **THEN** the subject SHALL be `"Hazel invited you to join Anime Club on BloomWatch"`

#### Scenario: HTML body contains brand colors and inviter context
- **WHEN** the email is composed
- **THEN** the HTML body SHALL contain the `#FF6B9D` brand color, the inviter's display name, the watch space name, and both action links

#### Scenario: Plain-text body is a readable fallback
- **WHEN** the email is composed
- **THEN** the plain-text body SHALL contain the inviter name, space name, and raw accept/decline URLs with no HTML markup

---

### Requirement: Invitation email is sent after successful invitation creation
The system SHALL call `IEmailSender.SendAsync` with the composed invitation email after the `Invitation` record has been successfully persisted. The email SHALL be sent to the `invitedEmail` address on the invitation.

#### Scenario: Email sent on successful invitation
- **WHEN** `POST /watchspaces/{id}/invitations` succeeds (HTTP 201)
- **THEN** `IEmailSender.SendAsync` SHALL have been called once with the invitee's email as the recipient

#### Scenario: Email failure after retries is surfaced in the response
- **WHEN** all `SmtpEmailSender` retry attempts fail
- **THEN** the API SHALL still return HTTP 201 with the invitation data, AND the response body SHALL include `"emailDeliveryFailed": true`

#### Scenario: Successful email send omits the failure flag
- **WHEN** `IEmailSender.SendAsync` completes without throwing
- **THEN** the 201 response body SHALL include `"emailDeliveryFailed": false`

#### Scenario: No email sent on rejected invitation
- **WHEN** `POST /watchspaces/{id}/invitations` returns a non-201 status (e.g., 409 or 422)
- **THEN** `IEmailSender.SendAsync` SHALL NOT be called
