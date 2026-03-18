## 1. AuthGuard Implementation

- [x] 1.1 Create `src/app/core/auth/guards/auth.guard.ts` with a functional `authGuard` (`CanActivateFn`) that checks `AuthService.isAuthenticated()` and redirects to `/login` when false
- [x] 1.2 Create `src/app/core/auth/guards/auth.guard.spec.ts` with unit tests: allow authenticated user, block unauthenticated user, block user with expired token, verify redirect to `/login`

## 2. GuestGuard Implementation

- [x] 2.1 Create `src/app/core/auth/guards/guest.guard.ts` with a functional `guestGuard` (`CanActivateFn`) that checks `AuthService.isAuthenticated()` and redirects to `/watch-spaces` when true
- [x] 2.2 Create `src/app/core/auth/guards/guest.guard.spec.ts` with unit tests: allow unauthenticated user, block authenticated user, verify redirect to `/watch-spaces`

## 3. Route Wiring

- [x] 3.1 Add `canActivate: [authGuard]` to the ShellLayout parent route in `app.routes.ts`
- [x] 3.2 Add `canActivate: [guestGuard]` to the `/login` and `/register` routes in `app.routes.ts`
- [x] 3.3 Verify the landing page route (`/`) has no guard applied

## 4. Verification

- [x] 4.1 Run all existing tests to confirm no regressions from route changes
- [x] 4.2 Run the new guard unit tests and confirm all pass
