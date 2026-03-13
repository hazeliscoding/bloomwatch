## ADDED Requirements

### Requirement: User can authenticate with email and password to receive a JWT
The system SHALL allow a registered user to authenticate by providing their email and password. On success, the system SHALL return a signed JWT access token (HS256) that encodes the user's ID, email, and display name. The token SHALL have an expiry of 1 hour from issuance.

#### Scenario: Successful login
- **WHEN** a client sends `POST /auth/login` with the email and password of an existing `Active` account and the password matches the stored hash
- **THEN** the system SHALL return HTTP 200 with a JSON body containing `accessToken` (the signed JWT string) and `expiresAt` (ISO 8601 UTC timestamp)

#### Scenario: Wrong password rejected
- **WHEN** a client sends `POST /auth/login` with a valid email but an incorrect password
- **THEN** the system SHALL return HTTP 401 Unauthorized and SHALL NOT indicate whether the email exists (to prevent user enumeration)

#### Scenario: Unknown email rejected
- **WHEN** a client sends `POST /auth/login` with an email address that does not match any registered account
- **THEN** the system SHALL return HTTP 401 Unauthorized with the same generic error message as an incorrect password

#### Scenario: Inactive account rejected
- **WHEN** a client sends `POST /auth/login` with valid credentials but the account's `AccountStatus` is not `Active`
- **THEN** the system SHALL return HTTP 401 Unauthorized with an error indicating the account is not active

### Requirement: JWT claims include user identity
The issued JWT SHALL contain claims sufficient to identify the user without a database lookup on every request.

#### Scenario: JWT payload contents
- **WHEN** the system issues an access token
- **THEN** the token payload SHALL include: `sub` (UserId as string), `email` (user's email), `display_name` (user's display name), `iat` (issued-at), and `exp` (expiry)

### Requirement: JWT middleware protects application routes
The system SHALL validate incoming JWT bearer tokens on all routes marked as `[Authorize]`. Requests with a missing, expired, or tampered token SHALL be rejected before reaching application logic.

#### Scenario: Valid token grants access
- **WHEN** a client sends a request to a protected route with a valid, non-expired JWT in the `Authorization: Bearer <token>` header
- **THEN** the system SHALL allow the request to proceed and SHALL make the user's claims available to the handler

#### Scenario: Missing token rejected on protected route
- **WHEN** a client sends a request to a protected route with no `Authorization` header
- **THEN** the system SHALL return HTTP 401 Unauthorized before reaching application logic

#### Scenario: Expired token rejected
- **WHEN** a client sends a request to a protected route with a JWT whose `exp` claim is in the past
- **THEN** the system SHALL return HTTP 401 Unauthorized
