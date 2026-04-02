## MODIFIED Requirements

### Requirement: AuthService stores JWT token and refresh token in localStorage
The `AuthService` SHALL persist the JWT access token in `localStorage` under the key `bloom_access_token` and the refresh token under the key `bloom_refresh_token`. It SHALL provide a `setTokens(accessToken: string, expiresAt: string, refreshToken: string, refreshTokenExpiresAt: string)` method that stores all four values, and a `clearTokens()` method that removes all four. The existing `setToken` method SHALL be removed.

#### Scenario: Tokens are stored after login
- **WHEN** `authService.setTokens('eyJ...', '2026-03-16T13:00:00Z', 'rt_...', '2026-04-16T13:00:00Z')` is called
- **THEN** `localStorage.getItem('bloom_access_token')` SHALL return `'eyJ...'`
- **AND** `localStorage.getItem('bloom_token_expires_at')` SHALL return `'2026-03-16T13:00:00Z'`
- **AND** `localStorage.getItem('bloom_refresh_token')` SHALL return `'rt_...'`
- **AND** `localStorage.getItem('bloom_refresh_token_expires_at')` SHALL return `'2026-04-16T13:00:00Z'`

#### Scenario: All tokens are cleared on logout
- **WHEN** `authService.clearTokens()` is called
- **THEN** `localStorage.getItem('bloom_access_token')` SHALL return `null`
- **AND** `localStorage.getItem('bloom_token_expires_at')` SHALL return `null`
- **AND** `localStorage.getItem('bloom_refresh_token')` SHALL return `null`
- **AND** `localStorage.getItem('bloom_refresh_token_expires_at')` SHALL return `null`

### Requirement: AuthService exposes reactive auth state and refresh token via signals
The `AuthService` SHALL expose an `isAuthenticated` signal (computed from access token presence and non-expiry OR refresh token presence and non-expiry), a `token` signal (current raw JWT string or `null`), and a `refreshToken` signal (current refresh token string or `null`).

#### Scenario: Authenticated state reflects valid access token
- **WHEN** a valid, non-expired access token exists in localStorage
- **THEN** `authService.isAuthenticated()` SHALL return `true`
- **AND** `authService.token()` SHALL return the token string

#### Scenario: Authenticated state reflects valid refresh token when access token is expired
- **WHEN** the access token exists but is expired
- **AND** a valid, non-expired refresh token exists in localStorage
- **THEN** `authService.isAuthenticated()` SHALL return `true`
- **AND** `authService.token()` SHALL return the expired access token string (the interceptor handles renewal)

#### Scenario: Authenticated state is false when both tokens are absent or expired
- **WHEN** no tokens exist in localStorage OR both access and refresh tokens are expired
- **THEN** `authService.isAuthenticated()` SHALL return `false`
- **AND** `authService.token()` SHALL return `null`

#### Scenario: refreshToken signal reflects stored value
- **WHEN** a refresh token is stored in localStorage
- **THEN** `authService.refreshToken()` SHALL return that token string

### Requirement: AuthService initializes state from localStorage on construction
The `AuthService` SHALL read any existing access token AND refresh token from localStorage when instantiated, so that page reloads preserve the authenticated session.

#### Scenario: Page reload preserves authentication when access token is valid
- **WHEN** the application is reloaded and a valid, non-expired access token exists in localStorage
- **THEN** `authService.isAuthenticated()` SHALL return `true` immediately after service initialization

#### Scenario: Page reload preserves authentication when only refresh token is valid
- **WHEN** the application is reloaded and the access token is expired but a valid refresh token exists in localStorage
- **THEN** `authService.isAuthenticated()` SHALL return `true` immediately after service initialization
- **AND** `authService.refreshToken()` SHALL return the stored refresh token string
