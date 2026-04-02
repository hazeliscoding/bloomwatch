## Context

BloomWatch currently issues 1-hour JWT access tokens with no renewal mechanism. Users who stay active longer than an hour are silently logged out — their next API call returns 401, which the frontend has no way to recover from. This is a known gap from the MVP cut. The `LoginUserCommandHandler` generates tokens via `IJwtTokenGenerator`; the `AuthService` (frontend) stores the JWT in `localStorage` and exposes an `isAuthenticated` signal.

## Goals / Non-Goals

**Goals:**
- Silent, user-invisible access token renewal using a long-lived refresh token
- Refresh tokens are single-use with rotation (each refresh issues a new pair)
- Secure server-side storage: only a SHA-256 hash of the refresh token is persisted
- Logout invalidates the refresh token
- No new packages required

**Non-Goals:**
- HttpOnly cookie transport — the frontend already uses `localStorage` for the access token; keeping refresh token in `localStorage` stays consistent (CSRF vs XSS is a known trade-off, see Risks)
- Token family detection / breach detection (deferred to a future security hardening issue)
- Refresh token reuse detection beyond single-use rotation

## Decisions

### 1. Opaque refresh token, stored hashed
**Decision**: Refresh tokens are cryptographically random opaque strings (32 bytes from `RandomNumberGenerator`, base64url-encoded). The server stores `SHA256(token)` in the database. The plain token is only ever returned in the HTTP response body.

**Why**: JWTs as refresh tokens are stateless and cannot be revoked without a denylist. An opaque token stored server-side enables hard revocation (logout, compromise). SHA-256 pre-image resistance means a database dump doesn't expose usable tokens.

**Alternatives considered**: Storing the plain token — simpler but unacceptably risky if the database is compromised.

### 2. Single-use token rotation
**Decision**: `POST /auth/refresh` atomically invalidates the submitted refresh token and issues a new access + refresh token pair.

**Why**: Rotation limits the window of opportunity if a refresh token is leaked. The old token is useless after one use.

**Alternatives considered**: Non-rotating refresh tokens — simpler but means a stolen token is usable indefinitely until expiry.

### 3. 30-day refresh token TTL
**Decision**: Refresh tokens expire 30 days after issuance.

**Why**: Long enough to cover reasonable inactivity gaps without requiring re-login. Short enough that compromised tokens have bounded lifetime even without revocation.

### 4. localStorage transport (body, not HttpOnly cookie)
**Decision**: Refresh token is returned in the response body and stored in `localStorage` under `bloom_refresh_token`.

**Why**: Stays consistent with existing access token storage pattern. Switching to HttpOnly cookies would require CSRF protection, cross-origin config changes, and a larger refactor than this issue scope.

**Trade-off**: XSS can steal tokens from `localStorage`. Mitigated by the existing CSP headers and short access token TTL. Accepted as a known, documented risk.

### 5. Frontend refresh strategy: proactive + 401 fallback
**Decision**: The `authInterceptor` will attempt a proactive refresh when `isAuthenticated()` is false but a valid refresh token exists. As a fallback, intercepted 401 responses also trigger a refresh-then-retry.

**Why**: Proactive refresh gives the best UX (no request failure visible to user). The 401 fallback handles edge cases like clock skew.

### 6. New `RefreshToken` entity in Identity domain
**Decision**: `RefreshToken` is a new aggregate root in `BloomWatch.Modules.Identity.Domain`. It holds `TokenHash`, `UserId`, `ExpiresAt`, `CreatedAt`, and a `IsRevoked` flag. A new `IRefreshTokenRepository` interface is added to the Domain layer; `EfRefreshTokenRepository` implements it in Infrastructure.

**Why**: Consistent with the existing DDD structure. Domain entity keeps the business rules (expiry check, revocation check) close to the data.

## Risks / Trade-offs

- **XSS token theft** → `localStorage` is accessible to injected scripts. Mitigated by strict CSP; accepted as scoped risk documented here.
- **Token rotation race condition** (two concurrent requests with the same refresh token) → The second request will fail with 401. Client should retry login. Low probability in a single-user session.
- **Database growth** → Expired refresh tokens accumulate. Mitigated: add a background cleanup (soft-delete + periodic purge query) in a follow-up; out of scope here.

## Migration Plan

1. Add EF Core migration to the Identity schema: new `refresh_tokens` table
2. Run `./scripts/apply-migrations.sh` on deploy
3. No changes to existing `Users` table or JWT config
4. Frontend release is backward-compatible: `bloom_refresh_token` key in `localStorage` will simply be absent for existing sessions; those sessions get a normal 401 → redirect to login
5. Rollback: remove the two new endpoints from the route registration; migration rollback removes the `refresh_tokens` table (no FK constraints on `users`)

## Open Questions

- None — all design decisions are resolved above.
