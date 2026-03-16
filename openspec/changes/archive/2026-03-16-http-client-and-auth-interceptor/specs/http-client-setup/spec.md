## ADDED Requirements

### Requirement: HttpClient is provisioned with functional interceptors
The application SHALL provide Angular's `HttpClient` via `provideHttpClient(withInterceptors([...]))` in `app.config.ts`, enabling all services to inject `HttpClient` for HTTP communication.

#### Scenario: HttpClient is available for injection
- **WHEN** any service or component injects `HttpClient`
- **THEN** Angular SHALL resolve a fully configured `HttpClient` instance without errors

### Requirement: Environment-based API base URL configuration
The application SHALL define environment configuration files that export an `apiBaseUrl` property. `environment.development.ts` SHALL point to the local dev server URL. `environment.ts` SHALL contain a production placeholder URL.

#### Scenario: Development environment provides local API URL
- **WHEN** the application is served in development mode
- **THEN** `environment.apiBaseUrl` SHALL resolve to the local backend dev server URL (e.g., `https://localhost:5001`)

#### Scenario: Production environment provides production API URL
- **WHEN** the application is built for production
- **THEN** `environment.apiBaseUrl` SHALL resolve to the configured production API URL

### Requirement: ApiService provides typed HTTP helper methods
The application SHALL provide an `ApiService` injectable that exposes typed `get<T>()`, `post<T>()`, `put<T>()`, `patch<T>()`, and `delete<T>()` methods. Each method SHALL auto-prefix request URLs with the environment's `apiBaseUrl`.

#### Scenario: GET request is prefixed with base URL
- **WHEN** a feature service calls `apiService.get<T>('/users/me')`
- **THEN** the underlying `HttpClient` SHALL make a GET request to `{apiBaseUrl}/users/me`

#### Scenario: POST request sends body and prefixes URL
- **WHEN** a feature service calls `apiService.post<T>('/auth/login', { email, password })`
- **THEN** the underlying `HttpClient` SHALL make a POST request to `{apiBaseUrl}/auth/login` with the provided JSON body

#### Scenario: Relative paths are joined correctly
- **WHEN** a feature service calls `apiService.get<T>('/api/anilist/search?q=naruto')`
- **THEN** the request URL SHALL be `{apiBaseUrl}/api/anilist/search?q=naruto` with no double slashes or malformed paths
