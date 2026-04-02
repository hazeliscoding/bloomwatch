## MODIFIED Requirements

### Requirement: User can authenticate with email and password to receive a JWT and refresh token
The system SHALL allow a registered user to authenticate by providing their email and password. On success, the system SHALL return a signed JWT access token (HS256) that encodes the user's ID, email, and display name, and SHALL also return an opaque refresh token. The access token SHALL have an expiry of 1 hour from issuance. The refresh token SHALL have an expiry of 30 days from issuance.

#### Scenario: Successful login
- **WHEN** a client sends `POST /auth/login` with the email and password of an existing `Active` account and the password matches the stored hash
- **THEN** the system SHALL return HTTP 200 with a JSON body containing `accessToken` (the signed JWT string), `expiresAt` (ISO 8601 UTC timestamp, 1 hour from now), `refreshToken` (an opaque token string), and `refreshTokenExpiresAt` (ISO 8601 UTC timestamp, 30 days from now)

#### Scenario: Wrong password rejected
- **WHEN** a client sends `POST /auth/login` with a valid email but an incorrect password
- **THEN** the system SHALL return HTTP 401 Unauthorized and SHALL NOT indicate whether the email exists (to prevent user enumeration)

#### Scenario: Unknown email rejected
- **WHEN** a client sends `POST /auth/login` with an email address that does not match any registered account
- **THEN** the system SHALL return HTTP 401 Unauthorized with the same generic error message as an incorrect password

#### Scenario: Inactive account rejected
- **WHEN** a client sends `POST /auth/login` with valid credentials but the account's `AccountStatus` is not `Active`
- **THEN** the system SHALL return HTTP 401 Unauthorized with an error indicating the account is not active
