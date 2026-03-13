## Why

The application currently has no mechanism for managing user identity, meaning there is no way to register users, authenticate them, or protect resources. Establishing a proper Identity module is a prerequisite for any feature that requires knowing who the caller is.

## What Changes

- Introduce a `User` aggregate in the Identity bounded context with fields for email, hashed password, display name, and account status
- Implement a user registration flow that validates input, hashes passwords, and persists new users
- Implement a JWT-based authentication flow that issues access tokens on successful login
- Expose HTTP endpoints for `POST /auth/register` and `POST /auth/login`
- Add JWT middleware for protecting downstream routes

## Capabilities

### New Capabilities

- `user-registration`: Accepts email + password, validates uniqueness, stores hashed credentials, returns the created user profile
- `user-authentication`: Validates email + password against stored credentials, issues a signed JWT access token on success

### Modified Capabilities

_(none — this is a greenfield Identity module)_

## Impact

- **New module**: `src/identity/` (domain, application, infrastructure layers)
- **New dependencies**: JWT library (e.g., `jsonwebtoken` or language-equivalent), password hashing library (e.g., `bcrypt`)
- **Database**: New `users` table / collection
- **HTTP layer**: Two new public endpoints; auth middleware wired into the router
- **Other modules**: Any future module requiring authentication will depend on the JWT middleware introduced here
