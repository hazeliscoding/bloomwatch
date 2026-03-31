## Why

When a watch space owner invites someone by email, the invitee has no way to know about it — there is no notification mechanism, so invitations sit unacknowledged unless communicated out-of-band. This is a P0 gap that blocks real-world onboarding (Closes #2).

## What Changes

- Add an `IEmailSender` abstraction to the SharedKernel or a new Email infrastructure layer, with a configurable SMTP implementation (MailKit)
- Add an invitation email template (plain-text + HTML) with the invitee's name, inviting user, watch space name, and an accept/decline link
- Wire the invitation-created path in the WatchSpaces module to trigger an email after the invitation record is persisted
- Add email provider configuration to `appsettings.json` (`Email:Smtp:*`)
- Add a no-op `NullEmailSender` for test and development environments

## Capabilities

### New Capabilities

- `email-infrastructure`: `IEmailSender` abstraction, MailKit SMTP implementation, `NullEmailSender` stub, DI registration, and `appsettings` configuration schema
- `invitation-email`: invitation email template (HTML + plain-text), trigger on invitation creation, recipient/sender address resolution

### Modified Capabilities

- `watch-space-invitations`: invitation creation now has a side-effect requirement — an email must be sent to the invitee's address after the record is persisted; failure to send must not roll back the invitation

## Impact

- **WatchSpaces module** — `SendInvitationCommandHandler` (Application layer) gains a dependency on `IEmailSender`; no domain model changes
- **New shared abstraction** — `IEmailSender` added to `BloomWatch.SharedKernel` or a new `BloomWatch.Infrastructure.Email` project
- **Configuration** — `appsettings.json` gains `Email:Smtp:Host`, `Email:Smtp:Port`, `Email:Smtp:Username`, `Email:Smtp:Password`, `Email:FromAddress`, `Email:FromName`
- **Tests** — integration tests inject `NullEmailSender`; unit tests for `SendInvitationCommandHandler` verify `IEmailSender.SendAsync` is called with correct parameters
- **Dependencies** — adds `MailKit` NuGet package
- **No frontend changes** required
