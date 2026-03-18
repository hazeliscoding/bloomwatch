## Context

BloomWatch's Angular 19 frontend has a complete auth token lifecycle: `AuthService` stores JWT + expiry in localStorage and exposes an `isAuthenticated` computed signal; the HTTP interceptor attaches tokens and handles 401 responses. However, there is no client-side route protection — users can navigate directly to any URL regardless of auth state. Story 7.4 closes this gap.

Current route structure in `app.routes.ts`:
- **MinimalLayout** group: `/` (landing), `/login`, `/register`
- **ShellLayout** group: `/` (dashboard), `/watch-spaces`, `/settings`, `/showcase`

## Goals / Non-Goals

**Goals:**
- Prevent unauthenticated users from accessing ShellLayout routes
- Prevent authenticated users from accessing login/register
- Leverage existing `AuthService.isAuthenticated()` signal for all checks
- Redirect users to sensible defaults on guard rejection

**Non-Goals:**
- Role-based or permission-based guards (no roles exist yet)
- Server-side route protection (backend already validates tokens per-request)
- Refresh-token flow or silent re-authentication
- Guard protection for the landing page (`/`) — it remains public

## Decisions

### 1. Functional guards over class-based guards

Use Angular's `CanActivateFn` pattern (standalone functions) rather than `@Injectable` class guards.

**Rationale:** Angular 19 favors functional guards — they are simpler, tree-shakable, and consistent with the project's standalone architecture. Class-based guards are soft-deprecated.

**Alternative considered:** Class-based `CanActivate` — rejected because it adds boilerplate with no benefit in this codebase.

### 2. Guard file location: `core/auth/guards/`

Place both guards in `src/app/core/auth/guards/` as standalone exported functions.

- `auth.guard.ts` → `authGuard`
- `guest.guard.ts` → `guestGuard`

**Rationale:** Keeps auth concerns co-located under `core/auth/`. Guards are cross-cutting infrastructure, not feature-specific.

### 3. Apply AuthGuard at the ShellLayout parent route level

Attach `canActivate: [authGuard]` to the ShellLayout parent route rather than individually on each child route.

**Rationale:** A single guard on the parent protects all current and future child routes automatically. Reduces maintenance burden and prevents accidental gaps.

### 4. Apply GuestGuard only to `/login` and `/register`

Attach `canActivate: [guestGuard]` to the login and register routes only. The landing page (`/`) remains accessible to all users.

**Rationale:** The landing page is a marketing/info page that should be visible regardless of auth state. Only auth-form routes need guest restriction.

### 5. Redirect targets

- **AuthGuard rejection** → `/login` (user needs to authenticate)
- **GuestGuard rejection** → `/watch-spaces` (authenticated user's natural entry point, consistent with post-login redirect in login component)

**Rationale:** Matches existing post-login navigation (`/watch-spaces`) and interceptor 401 redirect (`/login`).

### 6. Use `inject()` inside guard functions

Guards use Angular's `inject()` to get `AuthService` and `Router` — no constructor injection needed.

**Rationale:** Standard pattern for functional guards in Angular 19. Clean and testable.

## Risks / Trade-offs

- **[Client-side only]** Guards are UX-level protection, not security. The backend must (and does) independently validate tokens on every request. → Acceptable: guards prevent confusing UI states, not unauthorized data access.

- **[Race condition on token expiry]** A token could expire between guard check and API call. → Mitigated: the HTTP interceptor already handles 401 responses by clearing the token and redirecting to `/login`.

- **[Showcase route behind auth]** The `/showcase` route (design system reference) sits under ShellLayout and will require authentication. → Acceptable for now; if needed, it can be moved to MinimalLayout later.
