## ADDED Requirements

### Requirement: User can register with email and password
The system SHALL allow a new user to create a BloomWatch account by providing a unique email address, a password, and a display name. The system SHALL hash the password before persisting it and SHALL never store the plaintext password.

#### Scenario: Successful registration
- **WHEN** a client sends `POST /auth/register` with a valid email, password meeting complexity rules, and a non-empty display name
- **THEN** the system creates a new `User` aggregate, stores the hashed password in the `identity.users` table, and returns HTTP 201 with the new user's ID, display name, and email

#### Scenario: Duplicate email rejected
- **WHEN** a client sends `POST /auth/register` with an email address already associated with an existing account
- **THEN** the system SHALL return HTTP 409 Conflict with an error message indicating the email is already in use and SHALL NOT create a duplicate account

#### Scenario: Invalid email format rejected
- **WHEN** a client sends `POST /auth/register` with a value in the email field that is not a valid RFC 5321 email address
- **THEN** the system SHALL return HTTP 400 Bad Request with a validation error identifying the email field

#### Scenario: Password too short rejected
- **WHEN** a client sends `POST /auth/register` with a password shorter than 8 characters
- **THEN** the system SHALL return HTTP 400 Bad Request with a validation error on the password field

#### Scenario: Empty display name rejected
- **WHEN** a client sends `POST /auth/register` with a blank or whitespace-only display name
- **THEN** the system SHALL return HTTP 400 Bad Request with a validation error on the display name field

### Requirement: Registered user account is active by default
The system SHALL set a newly registered user's account status to `Active` upon creation. The system SHALL include an `IsEmailVerified` flag on the user, defaulting to `false`, to support future email verification without requiring a schema change.

#### Scenario: New user account status
- **WHEN** a user successfully registers
- **THEN** the persisted `User` aggregate SHALL have `AccountStatus = Active` and `IsEmailVerified = false`
