## Why

Users who forget their password have no recovery path — they are permanently locked out. This is a critical UX and retention gap (Roadmap §5) that must be closed before production. Closes #4.

## What Changes

- New `POST /auth/forgot-password` endpoint: accepts an email, generates a secure time-limited single-use reset token, persists it, and sends a password-reset email via the existing `IEmailSender` infrastructure
- New `POST /auth/reset-password` endpoint: accepts a token + new password, validates the token (expiry, single-use), hashes and updates the user's password, and invalidates the token
- Reset tokens expire after 1 hour and are invalidated immediately on use
- Rate limiting on `forgot-password` to prevent abuse (max 5 requests per email per hour)
- Frontend: "Forgot password?" link on login page → forgot-password page → confirmation message
- Frontend: Reset-password page reached via email link (`/auth/reset-password?token=<token>`)
- Clear error states for expired and already-used tokens

## Capabilities

### New Capabilities

- `forgot-password`: Backend endpoint and token-generation logic for initiating a password reset; email delivery via `IEmailSender`
- `reset-password`: Backend endpoint for validating a reset token and updating the user's password
- `password-reset-ui`: Angular pages for forgot-password and reset-password flows

### Modified Capabilities

- `user-authentication`: The login page gains a "Forgot password?" link — a UI-only addition, no requirement-level behavior change to the login spec itself

## Impact

- **Identity module** (backend): New `PasswordResetToken` entity in Domain; new `ResetTokenRepository`; new `ForgotPasswordCommandHandler` and `ResetPasswordCommandHandler` in Application; two new EF Core migration; two new endpoint registrations in `src/BloomWatch.Api/Modules/Identity/`
- **Email infrastructure**: No changes to `IEmailSender` — reuses existing abstraction; new password-reset HTML/text email template
- **Frontend**: Two new standalone Angular components under `features/auth/`; update `AuthService` to call the new endpoints; new routes in the auth feature
- **Database**: New `PasswordResetTokens` table in the Identity schema
- **Security**: Tokens are cryptographically random (32-byte `RandomNumberGenerator`), stored as SHA-256 hash (never plain), single-use, 1-hour TTL
