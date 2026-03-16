## Why

The Angular 21 frontend has routing, layouts, and a component library in place but no way to communicate with the .NET backend API. Every upcoming feature — login, registration, watch-space management, AniList search — is blocked until the frontend can make authenticated HTTP requests. Adding a centralized HTTP client layer with JWT token management and an auth interceptor is the critical next step that unblocks all feature integration work.

## What Changes

- Provision `HttpClient` via Angular's `provideHttpClient()` with functional interceptors
- Create an `AuthService` that stores, retrieves, and clears JWT tokens (localStorage-backed) and exposes reactive auth state via signals
- Create an `AuthInterceptor` (functional `HttpInterceptorFn`) that attaches `Authorization: Bearer <token>` headers to outgoing API requests
- Create an `ApiService` (or similar) that centralizes base-URL configuration and provides typed HTTP helper methods for the backend REST + AniList proxy endpoints
- Add environment configuration files for API base URL (development vs production)
- Handle 401 responses by clearing stale tokens and redirecting to the login route

## Capabilities

### New Capabilities
- `http-client-setup`: Angular HttpClient provisioning, environment-based API base URL config, and typed API helper service
- `auth-token-management`: JWT token storage, retrieval, expiry detection, and reactive auth-state signals
- `auth-interceptor`: Functional HTTP interceptor that attaches Bearer tokens and handles 401 unauthorized responses

### Modified Capabilities
_None — no existing spec-level requirements are changing._

## Impact

- **Code**: `src/BloomWatch.UI/` — new core services, interceptor, environment config, and app provider updates
- **Dependencies**: `@angular/common/http` (already available via `@angular/common`, just needs provisioning)
- **APIs**: Targets all backend endpoints under `/api/*` (Identity, WatchSpaces, AniListSync modules)
- **Systems**: localStorage for token persistence; router for 401 redirect to `/auth/login`
