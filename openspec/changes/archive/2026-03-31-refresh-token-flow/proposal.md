## Why

Access tokens expire after 1 hour, which forces users to re-authenticate on every session longer than an hour — a poor UX for an app designed for ongoing use. A refresh token flow allows silent token renewal without user intervention, while keeping access tokens short-lived for security. Closes #3.

## What Changes

- `POST /auth/login` response is extended to include a `refreshToken` (opaque string) and `refreshTokenExpiresAt` alongside the existing `accessToken` and `expiresAt`
- New endpoint `POST /auth/refresh` accepts a refresh token and returns a new access/refresh token pair (rotation)
- New endpoint `POST /auth/revoke` invalidates a refresh token (logout support)
- Backend stores hashed refresh tokens in a new `RefreshTokens` table in the Identity schema
- Refresh tokens are single-use with rotation: each use issues a new refresh token and invalidates the old one
- Refresh tokens expire after 30 days
- Frontend `AuthService` stores the refresh token, detects access token expiry, and silently calls `/auth/refresh` before re-issuing failed requests (via `authInterceptor` or a proactive scheduler)

## Capabilities

### New Capabilities

- `refresh-token-issuance`: Server-side issuance, storage (hashed), rotation, and revocation of refresh tokens; `RefreshTokens` EF table in Identity schema
- `silent-token-refresh`: Frontend auto-refresh logic — `AuthService` detects near-expiry or 401 responses, calls `/auth/refresh`, updates stored tokens, and retries the original request

### Modified Capabilities

- `user-authentication`: Login response now includes `refreshToken` and `refreshTokenExpiresAt` fields
- `auth-token-management`: `AuthService` must store, read, and clear the refresh token alongside the access token; expiry logic extended to cover refresh token lifetime

## Impact

- **Backend**: `Identity` module — new `RefreshToken` entity and EF migration, new `IRefreshTokenRepository`, two new command handlers (`RefreshTokenCommand`, `RevokeTokenCommand`), two new API endpoints
- **Frontend**: `AuthService` (signals update), `authInterceptor` (intercept 401 → refresh → retry), `ApiService` (no changes needed)
- **Security**: Refresh tokens are stored hashed (SHA-256) in the database; plain token is only ever returned in the HTTP response, never logged
- **Dependencies**: No new packages required
