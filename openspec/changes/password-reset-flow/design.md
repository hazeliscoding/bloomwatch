## Context

BloomWatch's Identity module already handles registration, login, and refresh-token rotation. Refresh tokens use a hash-on-store pattern (`SHA-256(plainToken)` → DB) with a `RefreshToken` domain entity; password-reset tokens follow the same security model. The existing `IEmailSender` / `SmtpEmailSender` abstraction (from `email-infrastructure`) is available in `BloomWatch.SharedKernel` — no new email plumbing is needed. The frontend auth feature has `AuthService` (signals-based) and an `authInterceptor`; both new pages are unauthenticated flows that sit alongside the existing login and registration pages.

## Goals / Non-Goals

**Goals:**
- Secure, time-limited, single-use password-reset tokens stored as hashes (never plain)
- Two backend endpoints: `POST /auth/forgot-password` and `POST /auth/reset-password`
- Rate limiting on `forgot-password` (5 requests / email / hour) to prevent abuse
- Two Angular pages: forgot-password form + reset-password form (linked from email)
- Clear, user-friendly error states for expired and already-used tokens
- Unit + integration test coverage for both handlers

**Non-Goals:**
- Account unlock / admin-triggered resets
- SMS-based or TOTP-based recovery
- Changing password while already logged in (that is a separate "change password" feature)
- Invalidating all existing sessions on password reset (out of scope for now)

## Decisions

### 1. Token storage: hash-on-store (SHA-256), never plain

**Decision:** Generate 32 random bytes via `RandomNumberGenerator.GetBytes(32)`, base64url-encode them for the email link, and store only `SHA-256(plainToken)` in the DB — exactly matching the `RefreshToken` precedent.

**Why:** The DB is a higher-value target than the email channel. If the `PasswordResetTokens` table leaks, attackers cannot derive valid tokens from hashes. Consistent with the refresh-token implementation already in the module, so the pattern is familiar to maintainers.

**Alternative considered:** Store the plain token encrypted with a server-side key. Rejected — adds key-management complexity with no meaningful security benefit over hashing, since the token is single-use and short-lived.

---

### 2. Single-use via `IsUsed` flag (not deletion)

**Decision:** Mark tokens `IsUsed = true` on consumption; do not `DELETE` the row.

**Why:** Deletion would make it impossible to distinguish "expired" from "already used" in error messages. Keeping the row lets us return a specific `400 Token has already been used` vs `400 Token has expired`, improving UX. Rows can be purged by a background job later.

---

### 3. Consistent 200 OK on `forgot-password` (no email-existence leak)

**Decision:** `POST /auth/forgot-password` always returns `200 OK` with `{ "message": "If that email is registered, a reset link has been sent." }` regardless of whether the email exists.

**Why:** Returning 404 for unknown emails is a user-enumeration vector. This is the same principle applied to the login `401` generic message in `user-authentication` spec.

---

### 4. Rate limiting via ASP.NET Core built-in `RateLimiter` (fixed-window per email)

**Decision:** Use `builder.Services.AddRateLimiter` with a fixed-window policy keyed on the normalised request email (lowercased). Window: 60 minutes, permit limit: 5.

**Why:** ASP.NET Core 8+ ships a built-in rate-limiter middleware; no extra package needed. Keying on email (not IP) prevents a user from being rate-limited by someone else sending requests from the same NAT. Fixed-window is simple and sufficient for abuse prevention at this scale.

**Alternative considered:** Redis-backed sliding window for distributed deployments. Rejected as over-engineering for the current single-instance deployment.

---

### 5. `PasswordResetToken` as a new domain entity in Identity Domain

**Decision:** Add `PasswordResetToken` to `BloomWatch.Modules.Identity.Domain/Aggregates/`, parallel to `RefreshToken`. Add `IPasswordResetTokenRepository` to `Repositories/`. Infrastructure adds the EF configuration and migration.

**Why:** Follows the existing DDD layering. The token has its own lifecycle (generate → validate → consume/expire) that belongs in the domain, not as a property hanging off `User`.

---

### 6. Frontend: two new standalone Angular components under `features/auth/`

**Decision:** `ForgotPasswordComponent` (route `/auth/forgot-password`) and `ResetPasswordComponent` (route `/auth/reset-password`). Both use `MinimalLayout`, matching the login/registration pages. `AuthService` gains two new methods: `forgotPassword(email)` and `resetPassword(token, newPassword)`.

**Why:** Consistent with the existing auth feature structure. `MinimalLayout` keeps the focus on the form with no nav distraction. Using `AuthService` as the single HTTP boundary keeps the pattern clean.

## Risks / Trade-offs

- **Token in URL query string** → Mitigation: Tokens are single-use and 1-hour TTL; server sets `Cache-Control: no-store` on the reset endpoint response. This is standard industry practice (e.g., GitHub, GitLab).
- **In-memory rate limiter resets on restart** → Mitigation: Acceptable at pre-production scale; can be replaced with a Redis-backed policy when horizontal scaling is needed.
- **No session invalidation on password reset** → Mitigation: Documented as a known gap (Non-Goal above); existing JWT access tokens (1-hour TTL) and refresh tokens continue to work. A follow-up issue can add "revoke all refresh tokens for user" logic.
- **Expired token cleanup** → Mitigation: No immediate background job; `IsUsed` and `ExpiresAt` columns are indexed so queries stay fast. Cleanup can be added as a scheduled `IHostedService` in a later iteration.

## Migration Plan

1. Add EF Core migration to Identity module: `AddPasswordResetTokensTable`
2. Run `./scripts/apply-migrations.sh` on deploy
3. Deploy backend — new endpoints are additive, no breaking changes
4. Deploy frontend — new routes are additive, existing auth routes unchanged
5. **Rollback**: drop the `PasswordResetTokens` table and revert the migration; remove the two endpoint registrations

## Open Questions

- Should the reset-password success response revoke all active refresh tokens for the user? (Security hardening — currently marked Non-Goal but worth revisiting before v0.2.0 tag.)
