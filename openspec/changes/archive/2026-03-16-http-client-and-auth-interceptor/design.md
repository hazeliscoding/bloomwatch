## Context

The Angular 21 frontend (`src/BloomWatch.UI/`) has routing, layouts, and a Bloom component library but no HTTP communication layer. The .NET backend exposes:

- `POST /auth/register` — public
- `POST /auth/login` — public, returns `{ accessToken, expiresAt }`
- `GET /users/me` — requires Bearer JWT
- `/watchspaces/*` — requires Bearer JWT
- `GET /api/anilist/search` — requires Bearer JWT

The app currently has `provideRouter(routes)` as its only provider. No `HttpClient`, no environment files, no service layer.

## Goals / Non-Goals

**Goals:**
- Provide a single, well-typed HTTP client configuration that all feature services will use
- Store and manage JWT tokens with reactive auth state exposed via Angular signals
- Automatically attach Bearer tokens to API requests via a functional interceptor
- Handle 401 responses globally (clear token, redirect to login)
- Configure environment-based API base URL for dev and production

**Non-Goals:**
- Refresh tokens — the backend issues 1-hour JWTs with no refresh endpoint; this will be addressed later if needed
- Route guards — auth guards for protected routes are a separate concern and will be their own change
- Feature-specific API calls (login form submission, watch-space CRUD) — those are downstream consumers of this layer

## Decisions

### 1. Functional interceptor over class-based interceptor

Use Angular's `HttpInterceptorFn` with `withInterceptors()` instead of the legacy class-based `HttpInterceptor`.

**Rationale:** Angular 21 encourages functional interceptors registered via `provideHttpClient(withInterceptors([...]))`. They are simpler, tree-shakable, and align with the standalone/signal-based architecture already in use. Class-based interceptors are legacy API.

### 2. localStorage for token persistence

Store the JWT in `localStorage` under a single key (`bloom_access_token`).

**Rationale:** The backend issues opaque JWT access tokens (no refresh tokens). localStorage survives page reloads which is essential for SPA navigation. sessionStorage was considered but would force re-login on every new tab. Cookie-based storage was considered but adds complexity (CSRF protection, httpOnly constraints) without benefit since the backend expects a Bearer header.

**Trade-off:** localStorage is accessible to XSS. This is an accepted risk mitigated by Angular's built-in XSS sanitization and CSP headers in production.

### 3. Signal-based auth state

Expose auth state via Angular signals (`isAuthenticated`, `currentToken`) rather than RxJS BehaviorSubjects.

**Rationale:** The project already uses Angular 21 signal-based APIs. Signals integrate naturally with the change-detection strategy and avoid manual subscription cleanup. Components can read `authService.isAuthenticated()` in templates directly.

### 4. Environment files for API base URL

Create `src/environments/environment.ts` and `environment.development.ts` using Angular's standard `fileReplacements` build config.

**Rationale:** Standard Angular pattern. `environment.development.ts` points to `https://localhost:{port}` for dev; `environment.ts` will hold the production URL (placeholder for now). The `ApiService` reads from the environment to prefix all requests.

### 5. Centralized ApiService over direct HttpClient injection

Create an `ApiService` that wraps `HttpClient` with typed `get<T>()`, `post<T>()`, `put<T>()`, `patch<T>()`, `delete<T>()` methods and auto-prefixes the base URL.

**Rationale:** Prevents every feature service from importing the environment and manually constructing URLs. Provides a single place to add cross-cutting concerns like request logging or error normalization later. Feature services inject `ApiService` instead of raw `HttpClient`.

### 6. File structure under `core/`

Place all HTTP infrastructure in `src/app/core/`:
- `core/auth/auth.service.ts` — token management + auth state signals
- `core/http/auth.interceptor.ts` — functional interceptor
- `core/http/api.service.ts` — typed HTTP wrapper
- `environments/` at `src/environments/` (Angular convention)

**Rationale:** Follows Angular convention of `core/` for singleton services and infrastructure. Keeps feature modules clean and focused on their domain.

## Risks / Trade-offs

- **[XSS token exposure]** → localStorage is XSS-accessible. Mitigated by Angular's built-in sanitization, strict CSP, and short-lived tokens (1 hour).
- **[No refresh token flow]** → Users must re-login after token expiry. Acceptable for MVP; can be layered in later without breaking changes.
- **[401 redirect loop]** → If the login endpoint itself returned 401, the interceptor could loop. Mitigated by skipping token attachment for requests to `/auth/*` paths.
- **[Race condition on token clear]** → Multiple parallel 401 responses could trigger multiple redirects. Mitigated by checking `isAuthenticated()` before navigating.
