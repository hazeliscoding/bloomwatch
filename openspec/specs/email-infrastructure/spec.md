## ADDED Requirements

### Requirement: IEmailSender abstraction exists in SharedKernel
The system SHALL provide an `IEmailSender` interface in `BloomWatch.SharedKernel` with a single `SendAsync(to: string, subject: string, htmlBody: string, plainTextBody: string)` method returning `Task`. Any module MAY depend on this interface without referencing another module.

#### Scenario: Interface is resolvable from DI
- **WHEN** the application starts with SMTP configuration present
- **THEN** `IEmailSender` SHALL resolve to `SmtpEmailSender`

---

### Requirement: SmtpEmailSender sends email via MailKit with retry
The system SHALL provide a `SmtpEmailSender` implementation that uses MailKit to deliver email over SMTP with TLS. It SHALL read configuration from `Email:Smtp:Host`, `Email:Smtp:Port`, `Email:Smtp:Username`, `Email:Smtp:Password`, `Email:FromAddress`, and `Email:FromName`. On transient failure, it SHALL retry up to 2 additional times (3 total attempts) with a 1-second delay between attempts using Polly.

#### Scenario: Successful send
- **WHEN** `SendAsync` is called with valid recipient, subject, and body
- **THEN** the system SHALL connect to the configured SMTP host, authenticate, send the message, and disconnect

#### Scenario: Transient failure triggers retry
- **WHEN** the first SMTP attempt fails with a transient error
- **THEN** the system SHALL retry up to 2 more times before giving up

#### Scenario: All retries exhausted — exception propagates to caller
- **WHEN** all 3 SMTP attempts fail
- **THEN** `SmtpEmailSender` SHALL throw so the caller (`SendInvitationCommandHandler`) can catch and set `emailDeliveryFailed: true`

---

### Requirement: NullEmailSender is a no-op stub for integration tests
The system SHALL provide a `NullEmailSender` that implements `IEmailSender` by logging the recipient and subject at `Debug` level and taking no other action. It SHALL only be registered in the `WebApplicationFactory` test host — never in development or production.

#### Scenario: No-op in tests
- **WHEN** `NullEmailSender.SendAsync` is called
- **THEN** no network call is made and no exception is thrown

---

### Requirement: Development environment uses Mailpit as local SMTP catch-all
`appsettings.Development.json` SHALL configure `Email:Smtp:Host` as `localhost`, `Email:Smtp:Port` as `1025`, and no username/password, pointing at a local Mailpit instance. Developers can view captured emails at `http://localhost:8025`.

#### Scenario: Dev emails captured by Mailpit
- **WHEN** the application runs in Development and an invitation is created
- **THEN** the email SHALL be delivered to Mailpit and visible at `http://localhost:8025` without leaving the local machine

---

### Requirement: Email configuration schema is defined in appsettings
The `appsettings.json` file SHALL contain an `Email` section with placeholder values: `Smtp:Host` (empty), `Smtp:Port` (587), `Smtp:Username` (empty), `Smtp:Password` (empty), `FromAddress`, `FromName`, and `App:BaseUrl`.

#### Scenario: Production config via environment variables
- **WHEN** `Email__Smtp__Host`, `Email__Smtp__Password`, and `Email__FromAddress` environment variables are set
- **THEN** the application SHALL use those values, overriding `appsettings.json`
