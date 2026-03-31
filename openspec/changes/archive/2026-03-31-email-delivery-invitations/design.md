## Context

Currently, `SendInvitationCommandHandler` creates an `Invitation` record and returns. The invitee has no automatic notification — they must be told about the invitation out-of-band. This blocks real onboarding for any pair where both users don't already have accounts and a direct communication channel.

The WatchSpaces module owns invitations. Email infrastructure is cross-cutting and should not be owned by any single domain module.

## Goals / Non-Goals

**Goals:**
- Send an email to the invitee when an invitation is successfully persisted
- Retry delivery up to 2 times before giving up
- Surface delivery failures to the owner in the 201 response so the frontend can show a warning
- Use Mailpit locally so emails are visible during development without leaving the machine
- Keep the email abstraction simple — no delivery tracking or webhooks in v1

**Non-Goals:**
- Email delivery receipts or open tracking
- Outbox/queued delivery (no message bus in this project yet)
- Invitee registration via email link (Issue #6 — separate change)
- Other email types (password reset, etc. — those come in later changes)
- HTML templating engine (Razor/Fluid) — inline template strings are sufficient for v1

## Decisions

### 1. Where does `IEmailSender` live?

**Decision:** Add to `BloomWatch.SharedKernel`.

`IEmailSender` is a single interface (`SendAsync(to, subject, htmlBody, plainBody)`). Putting it in SharedKernel means any module can depend on it without cross-module coupling. A dedicated `BloomWatch.Infrastructure.Email` project would be overkill for one interface and one implementation.

**Alternatives considered:**
- *WatchSpaces.Application* — too narrow; other modules (Identity for password reset, etc.) will need it later
- *Separate project* — adds a project reference just for one interface; premature

### 2. SMTP library

**Decision:** MailKit via `MimeKit`.

MailKit is the de-facto .NET SMTP library (used by ASP.NET Identity itself), has async support, and handles TLS correctly. No third-party SaaS required — any SMTP relay (Mailpit locally, Mailgun/Postmark in prod) works.

**Alternatives considered:**
- *System.Net.Mail* — lacks async and has known TLS issues on Linux
- *SendGrid/Postmark SDK* — vendor lock-in; MailKit + any SMTP relay is more portable

### 3. Email-send failure handling

**Decision:** Retry up to 2 times (3 total attempts) with a 1-second delay via Polly, then log the error and return `emailDeliveryFailed: true` in the 201 response body. The invitation record is never rolled back.

The invitation IS the authoritative record. The owner seeing `emailDeliveryFailed: true` can notify the invitee manually — this is a recoverable UX situation. Rolling back the invitation on SMTP failure would be strictly worse.

**Alternatives considered:**
- *Silent log-only* — owner has no feedback; they'd never know email failed
- *Outbox pattern* — correct for guaranteed delivery, but requires a background worker. Deferred until message reliability becomes a hard requirement
- *Throw and rollback* — rejected; invitation data loss is worse than a missed email

### 4. Dev email environment

**Decision:** Mailpit running locally on `localhost:1025` (SMTP) / `localhost:8025` (web UI).

Mailpit is a zero-config local SMTP catch-all — emails are captured and visible at `http://localhost:8025` without leaving the machine. `appsettings.Development.json` points `Email:Smtp:Host` to `localhost:1025` (no auth). A `NullEmailSender` is retained only for integration tests via `WebApplicationFactory`.

**Alternatives considered:**
- *Ethereal* — requires a web account; Mailpit is fully local and Docker-based
- *NullEmailSender in dev* — emails vanish silently; template changes can't be verified without running e2e

### 5. Email template approach

**Decision:** Inline C# string interpolation in `InvitationEmailComposer` with kawaii-branded inline CSS.

The HTML body uses inline CSS styled to match the BloomWatch brand: hot-pink/lilac palette (`#FF6B9D`, `#C084FC`), Nunito font stack, rounded buttons, and a simple header with the BloomWatch name. Plain-text fallback strips all styling. A full templating engine is unnecessary for a single template.

## Risks / Trade-offs

- **SMTP credentials in config** → Use environment variable overrides (`Email__Smtp__Password`) in production; never commit real credentials. The default `appsettings.json` ships with empty/placeholder values.
- **Mailpit not running** → If a developer forgets to start Mailpit, SMTP connect fails, retries exhaust, and the API returns `emailDeliveryFailed: true`. The invitation still works; the dev just won't see the email. Log message at startup if Mailpit is unreachable would help (future improvement).
- **Inline retry adds latency** → 3 attempts × ~1s each = up to ~3s added to the invitation endpoint if SMTP is down. Acceptable for an owner-initiated action; not on a hot read path.
- **Accept/decline links require a known frontend URL** → Add `App:BaseUrl` to config (e.g. `http://localhost:4200`). The link format is `{baseUrl}/invitations/{token}/accept`.

## Migration Plan

1. Add `IEmailSender` to `BloomWatch.SharedKernel`; add `NullEmailSender` (test-only stub)
2. Add `MailKit` package; create `SmtpEmailSender` with Polly retry in `BloomWatch.Api`
3. Register via `AddEmailServices(configuration)` in `Program.cs`
4. Update `appsettings.json` (placeholder values) and `appsettings.Development.json` (Mailpit)
5. Add `InvitationEmailComposer` with kawaii HTML template to WatchSpaces.Application
6. Inject `IEmailSender` into `SendInvitationCommandHandler`; add `emailDeliveryFailed` to response
7. Add Mailpit setup note to README / dev setup docs

Rollback: if SMTP is misconfigured, invitations still work — `emailDeliveryFailed: true` is returned. No schema changes required.

## Open Questions

- Should the accept/decline link in the email point directly to the API token endpoint, or to a frontend deep-link? **Assumed:** frontend deep-link (`{baseUrl}/invitations/{token}`), letting the frontend call the API. If the frontend route doesn't exist yet, a future change (Issue #6) will add it.
