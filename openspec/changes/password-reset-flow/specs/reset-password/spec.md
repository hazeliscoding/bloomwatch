## ADDED Requirements

### Requirement: User can set a new password using a valid reset token
The system SHALL expose `POST /auth/reset-password` that accepts `{ "token": "<plainToken>", "newPassword": "<password>" }`. It SHALL hash the submitted token, look up the record by hash, validate it (exists, not used, not expired), hash the new password with bcrypt, update the user's `PasswordHash`, and mark the token `IsUsed = true` — all in a single atomic transaction.

#### Scenario: Valid token with acceptable password succeeds
- **WHEN** a client sends `POST /auth/reset-password` with a valid, non-expired, unused token and a password meeting the minimum requirements
- **THEN** the system SHALL return HTTP 200 with `{ "message": "Password has been reset successfully." }`
- **AND** the user's `PasswordHash` SHALL be updated to the bcrypt hash of the new password
- **AND** the `PasswordResetToken` record SHALL have `IsUsed = true`

#### Scenario: Expired token is rejected
- **WHEN** a client sends `POST /auth/reset-password` with a token whose `ExpiresAt` is in the past
- **THEN** the system SHALL return HTTP 400 Bad Request with `{ "error": "Reset link has expired. Please request a new one." }`

#### Scenario: Already-used token is rejected
- **WHEN** a client sends `POST /auth/reset-password` with a token that has `IsUsed = true`
- **THEN** the system SHALL return HTTP 400 Bad Request with `{ "error": "Reset link has already been used. Please request a new one." }`

#### Scenario: Unknown token is rejected
- **WHEN** a client sends `POST /auth/reset-password` with a token hash that does not match any record
- **THEN** the system SHALL return HTTP 400 Bad Request with `{ "error": "Invalid reset link. Please request a new one." }`

#### Scenario: Token is invalidated atomically with password update
- **WHEN** the password update succeeds
- **THEN** `IsUsed = true` SHALL be persisted in the same database transaction as the password hash update, preventing a second use even under concurrent requests

---

### Requirement: New password meets minimum strength requirements
The submitted `newPassword` SHALL be validated before processing: minimum 8 characters, at least one uppercase letter, one lowercase letter, and one digit.

#### Scenario: Password too short is rejected
- **WHEN** a client sends `POST /auth/reset-password` with a `newPassword` shorter than 8 characters
- **THEN** the system SHALL return HTTP 400 Bad Request with a validation error before looking up the token

#### Scenario: Password meeting all requirements is accepted
- **WHEN** a client sends `POST /auth/reset-password` with a `newPassword` of 8+ characters containing at least one uppercase letter, one lowercase letter, and one digit
- **THEN** the validation step SHALL pass and the system SHALL proceed to token lookup

---

### Requirement: PasswordResetToken domain entity enforces validity rules
The `PasswordResetToken` domain entity SHALL expose an `IsValid()` method that returns `true` only when `IsUsed = false` AND `ExpiresAt > UtcNow`.

#### Scenario: Unused, non-expired token is valid
- **WHEN** `IsValid()` is called on a token with `IsUsed = false` and `ExpiresAt` in the future
- **THEN** it SHALL return `true`

#### Scenario: Used token is not valid
- **WHEN** `IsValid()` is called on a token with `IsUsed = true`
- **THEN** it SHALL return `false`

#### Scenario: Expired token is not valid
- **WHEN** `IsValid()` is called on a token with `ExpiresAt` in the past
- **THEN** it SHALL return `false`
