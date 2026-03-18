## Why

The app currently has no client-side route protection. Unauthenticated users can navigate directly to protected pages (dashboard, watch-spaces, settings), and authenticated users still see the login/register pages. Story 7.4 adds Angular route guards to enforce access control at the routing level, completing the frontend auth flow started in Stories 7.2 and 7.3.

## What Changes

- Add an **AuthGuard** (functional `canActivate`) that blocks unauthenticated access to all ShellLayout routes (`/dashboard`, `/watch-spaces/**`, `/settings`) and redirects to `/login`
- Add a **GuestGuard** (functional `canActivate`) that redirects already-authenticated users away from `/login` and `/register` to `/watch-spaces`
- Both guards validate token presence **and** expiration using `AuthService.isAuthenticated()`
- Wire guards into `app.routes.ts` route definitions
- Token expiry during a session triggers redirect on next protected navigation attempt

## Capabilities

### New Capabilities
- `auth-route-guards`: Functional Angular route guards (AuthGuard + GuestGuard) that enforce authentication-based access control on protected and public-only routes

### Modified Capabilities

_None — this adds routing-level enforcement without changing existing auth token management or interceptor behavior._

## Impact

- **Routes**: `app.routes.ts` gains `canActivate` arrays on the ShellLayout and MinimalLayout auth route groups
- **New files**: Two guard functions under `core/auth/guards/`
- **Tests**: Unit tests for both guards covering allow, block, redirect, and token-expiry scenarios
- **No backend changes**: Guards rely entirely on existing client-side `AuthService` state
- **No breaking changes**: Existing navigation flows (post-login redirect, 401 interceptor redirect) are unaffected
