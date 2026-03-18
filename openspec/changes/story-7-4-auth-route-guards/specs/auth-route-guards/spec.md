## ADDED Requirements

### Requirement: AuthGuard blocks unauthenticated access to protected routes
The system SHALL prevent unauthenticated users from navigating to any route under the ShellLayout group (`/dashboard`, `/watch-spaces/**`, `/settings`, `/showcase`). When access is blocked, the system SHALL redirect the user to `/login`.

#### Scenario: Unauthenticated user navigates to a protected route
- **WHEN** a user with no valid auth token navigates to `/watch-spaces`
- **THEN** the system SHALL redirect the user to `/login` and SHALL NOT render the target route

#### Scenario: Authenticated user navigates to a protected route
- **WHEN** a user with a valid, non-expired auth token navigates to `/watch-spaces`
- **THEN** the system SHALL allow navigation and render the target route

#### Scenario: User with expired token navigates to a protected route
- **WHEN** a user whose stored token has passed its `expiresAt` timestamp navigates to `/settings`
- **THEN** the system SHALL treat the user as unauthenticated and redirect to `/login`

#### Scenario: Token expires during active session
- **WHEN** a user's token expires while they are on a protected page and they then navigate to another protected route
- **THEN** the system SHALL redirect the user to `/login` on that navigation attempt

### Requirement: GuestGuard redirects authenticated users away from auth pages
The system SHALL prevent authenticated users from accessing `/login` and `/register`. When an authenticated user attempts to access these routes, the system SHALL redirect them to `/watch-spaces`.

#### Scenario: Authenticated user navigates to login page
- **WHEN** a user with a valid auth token navigates to `/login`
- **THEN** the system SHALL redirect the user to `/watch-spaces` and SHALL NOT render the login page

#### Scenario: Authenticated user navigates to register page
- **WHEN** a user with a valid auth token navigates to `/register`
- **THEN** the system SHALL redirect the user to `/watch-spaces` and SHALL NOT render the register page

#### Scenario: Unauthenticated user navigates to login page
- **WHEN** a user with no valid auth token navigates to `/login`
- **THEN** the system SHALL allow navigation and render the login page

#### Scenario: Unauthenticated user navigates to register page
- **WHEN** a user with no valid auth token navigates to `/register`
- **THEN** the system SHALL allow navigation and render the register page

### Requirement: Guards validate token presence and expiration
Both AuthGuard and GuestGuard SHALL determine authentication status by reading `AuthService.isAuthenticated()`, which checks that a token exists AND that the stored `expiresAt` timestamp has not passed.

#### Scenario: Token present but expired
- **WHEN** a guard checks authentication and the token exists in localStorage but `expiresAt` is in the past
- **THEN** the guard SHALL treat the user as unauthenticated

#### Scenario: No token stored
- **WHEN** a guard checks authentication and no token exists in localStorage
- **THEN** the guard SHALL treat the user as unauthenticated

#### Scenario: Token present and valid
- **WHEN** a guard checks authentication and a token exists with a future `expiresAt` timestamp
- **THEN** the guard SHALL treat the user as authenticated

### Requirement: Landing page remains publicly accessible
The landing page at `/` SHALL remain accessible to both authenticated and unauthenticated users without any guard restrictions.

#### Scenario: Unauthenticated user visits landing page
- **WHEN** an unauthenticated user navigates to `/`
- **THEN** the system SHALL render the landing page without redirect

#### Scenario: Authenticated user visits landing page
- **WHEN** an authenticated user navigates to `/`
- **THEN** the system SHALL render the landing page without redirect
