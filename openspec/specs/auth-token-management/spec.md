## ADDED Requirements

### Requirement: AuthService stores JWT token in localStorage
The `AuthService` SHALL persist the JWT access token in `localStorage` under the key `bloom_access_token`. It SHALL provide a `setToken(token: string, expiresAt: string)` method that stores the token and expiry, and a `clearToken()` method that removes them.

#### Scenario: Token is stored after login
- **WHEN** `authService.setToken('eyJ...', '2026-03-16T13:00:00Z')` is called
- **THEN** `localStorage.getItem('bloom_access_token')` SHALL return `'eyJ...'`
- **AND** the expiry SHALL be persisted under `bloom_token_expires_at`

#### Scenario: Token is cleared on logout
- **WHEN** `authService.clearToken()` is called
- **THEN** `localStorage.getItem('bloom_access_token')` SHALL return `null`
- **AND** `localStorage.getItem('bloom_token_expires_at')` SHALL return `null`

### Requirement: AuthService exposes reactive auth state via signals
The `AuthService` SHALL expose an `isAuthenticated` signal (computed from token presence and non-expiry) and a `token` signal (the current raw JWT string or `null`).

#### Scenario: Authenticated state reflects valid token
- **WHEN** a valid, non-expired token exists in localStorage
- **THEN** `authService.isAuthenticated()` SHALL return `true`
- **AND** `authService.token()` SHALL return the token string

#### Scenario: Authenticated state reflects missing token
- **WHEN** no token exists in localStorage
- **THEN** `authService.isAuthenticated()` SHALL return `false`
- **AND** `authService.token()` SHALL return `null`

#### Scenario: Authenticated state reflects expired token
- **WHEN** a token exists in localStorage but `bloom_token_expires_at` is in the past
- **THEN** `authService.isAuthenticated()` SHALL return `false`

### Requirement: AuthService initializes state from localStorage on construction
The `AuthService` SHALL read any existing token from localStorage when instantiated, so that page reloads preserve the authenticated session.

#### Scenario: Page reload preserves authentication
- **WHEN** the application is reloaded and a valid, non-expired token exists in localStorage
- **THEN** `authService.isAuthenticated()` SHALL return `true` immediately after service initialization
