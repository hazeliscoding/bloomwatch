## ADDED Requirements

### Requirement: Server issues a refresh token alongside the access token on login
When a user successfully authenticates, the system SHALL generate a cryptographically random refresh token (32 bytes, base64url-encoded), persist a SHA-256 hash of it in the `refresh_tokens` table with a 30-day expiry, and return the plain token in the login response.

#### Scenario: Refresh token is issued on successful login
- **WHEN** a client sends `POST /auth/login` with valid credentials
- **THEN** the response SHALL include `refreshToken` (a non-empty string) and `refreshTokenExpiresAt` (ISO 8601 UTC, approximately 30 days in the future)

#### Scenario: Refresh token hash is stored, not the plain token
- **WHEN** a refresh token is issued
- **THEN** the `refresh_tokens` table SHALL contain `SHA256(refreshToken)` and NOT the plain token string

### Requirement: Client can exchange a refresh token for a new token pair
The system SHALL provide `POST /auth/refresh` that accepts a `{ "refreshToken": "..." }` body, validates the token, rotates it (revokes the old, issues new), and returns a fresh access + refresh token pair.

#### Scenario: Valid refresh token returns new token pair
- **WHEN** a client sends `POST /auth/refresh` with a valid, non-expired, non-revoked refresh token
- **THEN** the system SHALL return HTTP 200 with `accessToken`, `expiresAt`, `refreshToken` (new), and `refreshTokenExpiresAt` (new, 30 days from now)
- **AND** the submitted refresh token SHALL be marked revoked in the database
- **AND** a new refresh token record SHALL be created for the same user

#### Scenario: Expired refresh token is rejected
- **WHEN** a client sends `POST /auth/refresh` with a refresh token whose `ExpiresAt` is in the past
- **THEN** the system SHALL return HTTP 401 Unauthorized

#### Scenario: Revoked refresh token is rejected
- **WHEN** a client sends `POST /auth/refresh` with a refresh token that has `IsRevoked = true`
- **THEN** the system SHALL return HTTP 401 Unauthorized

#### Scenario: Unknown refresh token is rejected
- **WHEN** a client sends `POST /auth/refresh` with a token hash that does not match any record
- **THEN** the system SHALL return HTTP 401 Unauthorized

### Requirement: Client can revoke a refresh token (logout)
The system SHALL provide `POST /auth/revoke` that accepts a `{ "refreshToken": "..." }` body and marks the token as revoked, invalidating it for future use.

#### Scenario: Valid token is successfully revoked
- **WHEN** a client sends `POST /auth/revoke` with a valid refresh token
- **THEN** the system SHALL return HTTP 204 No Content
- **AND** the token's `IsRevoked` flag SHALL be set to `true`

#### Scenario: Revoke is idempotent for unknown tokens
- **WHEN** a client sends `POST /auth/revoke` with a token that does not exist or is already revoked
- **THEN** the system SHALL return HTTP 204 No Content (no error exposed to prevent token enumeration)

### Requirement: RefreshToken domain entity enforces validity rules
The `RefreshToken` domain entity SHALL expose an `IsValid()` method that returns `true` only when the token is not revoked AND has not expired.

#### Scenario: Non-revoked, non-expired token is valid
- **WHEN** `IsRevoked` is `false` and `ExpiresAt` is in the future
- **THEN** `IsValid()` SHALL return `true`

#### Scenario: Revoked token is invalid
- **WHEN** `IsRevoked` is `true`
- **THEN** `IsValid()` SHALL return `false` regardless of expiry

#### Scenario: Expired token is invalid
- **WHEN** `ExpiresAt` is in the past
- **THEN** `IsValid()` SHALL return `false` regardless of revocation state
