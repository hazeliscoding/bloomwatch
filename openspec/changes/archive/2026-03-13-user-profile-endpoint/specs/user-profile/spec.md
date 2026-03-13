## ADDED Requirements

### Requirement: Authenticated user can retrieve their own profile
The system SHALL allow an authenticated user to retrieve their profile information by calling `GET /users/me`. The response SHALL include the user's ID, email, display name, account status, email verification status, and account creation timestamp.

#### Scenario: Successful profile retrieval
- **WHEN** a client sends `GET /users/me` with a valid, non-expired JWT in the `Authorization: Bearer <token>` header
- **THEN** the system SHALL return HTTP 200 with a JSON body containing `userId` (string), `email` (string), `displayName` (string), `accountStatus` (string), `isEmailVerified` (boolean), and `createdAtUtc` (ISO 8601 UTC timestamp)

#### Scenario: Profile data matches persisted user record
- **WHEN** the system processes a valid `GET /users/me` request
- **THEN** all returned fields SHALL reflect the current state of the `User` aggregate identified by the JWT `sub` claim, not cached or stale data

#### Scenario: Unauthenticated request rejected
- **WHEN** a client sends `GET /users/me` without an `Authorization` header
- **THEN** the system SHALL return HTTP 401 Unauthorized before reaching application logic

#### Scenario: Expired token rejected
- **WHEN** a client sends `GET /users/me` with a JWT whose `exp` claim is in the past
- **THEN** the system SHALL return HTTP 401 Unauthorized

#### Scenario: User no longer exists
- **WHEN** a client sends `GET /users/me` with a valid JWT, but the user identified by the `sub` claim no longer exists in the database
- **THEN** the system SHALL return HTTP 404 Not Found
