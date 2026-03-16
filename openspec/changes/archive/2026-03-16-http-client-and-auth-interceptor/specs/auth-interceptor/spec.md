## ADDED Requirements

### Requirement: Auth interceptor attaches Bearer token to API requests
The auth interceptor SHALL attach an `Authorization: Bearer <token>` header to all outgoing HTTP requests when a valid token is available in `AuthService`.

#### Scenario: Authenticated request includes Bearer header
- **WHEN** `authService.token()` returns a valid JWT string
- **AND** an HTTP request is made via `HttpClient`
- **THEN** the request SHALL include the header `Authorization: Bearer <token>`

#### Scenario: Unauthenticated request has no Authorization header
- **WHEN** `authService.token()` returns `null`
- **AND** an HTTP request is made via `HttpClient`
- **THEN** the request SHALL NOT include an `Authorization` header

### Requirement: Auth interceptor skips token for auth endpoints
The auth interceptor SHALL NOT attach a Bearer token to requests targeting `/auth/*` paths (login, register) to avoid sending stale or irrelevant tokens on public endpoints.

#### Scenario: Login request has no Authorization header
- **WHEN** a POST request is made to a URL containing `/auth/login`
- **THEN** the request SHALL NOT include an `Authorization` header, regardless of token state

#### Scenario: Register request has no Authorization header
- **WHEN** a POST request is made to a URL containing `/auth/register`
- **THEN** the request SHALL NOT include an `Authorization` header, regardless of token state

### Requirement: Auth interceptor handles 401 responses
The auth interceptor SHALL intercept 401 Unauthorized responses from the backend. Upon receiving a 401, it SHALL clear the stored token via `AuthService.clearToken()` and redirect the user to the `/login` route.

#### Scenario: 401 response clears token and redirects
- **WHEN** the backend returns a 401 Unauthorized response
- **THEN** `authService.clearToken()` SHALL be called
- **AND** the router SHALL navigate to `/login`

#### Scenario: 401 does not trigger redirect if already unauthenticated
- **WHEN** the backend returns a 401 Unauthorized response
- **AND** `authService.isAuthenticated()` is already `false`
- **THEN** the router SHALL still navigate to `/login`
- **BUT** `clearToken()` SHALL still be called (idempotent)

### Requirement: Auth interceptor is registered via provideHttpClient
The auth interceptor function SHALL be registered in `app.config.ts` using `provideHttpClient(withInterceptors([authInterceptor]))`.

#### Scenario: Interceptor is active on all HttpClient requests
- **WHEN** any service makes an HTTP request via the injected `HttpClient`
- **THEN** the request SHALL pass through the auth interceptor pipeline
