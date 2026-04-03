## ADDED Requirements

### Requirement: User can request a password reset email
The system SHALL expose `POST /auth/forgot-password` that accepts `{ "email": "<address>" }`. If the email belongs to an `Active` account, the system SHALL generate a cryptographically random 32-byte reset token, store its SHA-256 hash in the `PasswordResetTokens` table with a 1-hour expiry and `IsUsed = false`, and send a password-reset email to the address. The system SHALL return `200 OK` regardless of whether the email is registered, to prevent user enumeration.

#### Scenario: Email belongs to an active account
- **WHEN** a client sends `POST /auth/forgot-password` with an email that matches an `Active` user account
- **THEN** the system SHALL return HTTP 200 with `{ "message": "If that email is registered, a reset link has been sent." }`
- **AND** a `PasswordResetToken` record SHALL be created with `SHA256(plainToken)`, `ExpiresAt = UtcNow + 1 hour`, `IsUsed = false`
- **AND** the password-reset email SHALL be sent to the provided address containing the plain token in a reset URL

#### Scenario: Email does not exist
- **WHEN** a client sends `POST /auth/forgot-password` with an email address that has no matching account
- **THEN** the system SHALL return HTTP 200 with `{ "message": "If that email is registered, a reset link has been sent." }`
- **AND** no `PasswordResetToken` record SHALL be created and no email SHALL be sent

#### Scenario: Email belongs to an inactive account
- **WHEN** a client sends `POST /auth/forgot-password` with an email that matches a non-`Active` account
- **THEN** the system SHALL return HTTP 200 with the same generic message and SHALL NOT send an email (to avoid leaking account status)

#### Scenario: Invalid email format is rejected
- **WHEN** a client sends `POST /auth/forgot-password` with a value that is not a valid email address
- **THEN** the system SHALL return HTTP 400 Bad Request with a validation error

---

### Requirement: Forgot-password endpoint is rate-limited per email
The system SHALL enforce a fixed-window rate limit of 5 requests per normalised (lowercased) email per 60-minute window on `POST /auth/forgot-password`.

#### Scenario: Requests within the limit succeed
- **WHEN** a client sends 5 or fewer `POST /auth/forgot-password` requests for the same email within a 60-minute window
- **THEN** each request SHALL return HTTP 200

#### Scenario: Requests exceeding the limit are rejected
- **WHEN** a client sends a 6th `POST /auth/forgot-password` request for the same email within the same 60-minute window
- **THEN** the system SHALL return HTTP 429 Too Many Requests

---

### Requirement: Reset token is stored as SHA-256 hash, never plain
The system SHALL store only `SHA-256(base64url(randomBytes))` in the `PasswordResetTokens` table. The plain token SHALL only appear in the email link and SHALL never be persisted.

#### Scenario: Token hash is stored, not plain token
- **WHEN** a password-reset token is generated
- **THEN** the `PasswordResetTokens` table SHALL contain the SHA-256 hash of the token AND NOT the plain token string

---

### Requirement: Password-reset email contains a secure link
The system SHALL send an HTML + plain-text email with a link of the form `<AppBaseUrl>/auth/reset-password?token=<plainToken>`. The email SHALL include the expiry time (1 hour from request) and a note that the link is single-use.

#### Scenario: Email link format is correct
- **WHEN** a reset token is generated for an active user
- **THEN** the sent email SHALL contain a URL with the plain token as a `token` query parameter pointing to `<AppBaseUrl>/reset-password`
