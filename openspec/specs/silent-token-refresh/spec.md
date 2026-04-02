## ADDED Requirements

### Requirement: AuthInterceptor silently refreshes tokens before expired requests are sent
The `authInterceptor` SHALL detect when the stored access token is absent or expired but a refresh token is present, call `POST /auth/refresh`, update the stored tokens, and then dispatch the original request with the new access token — all without user intervention.

#### Scenario: Proactive refresh before a request when access token is expired
- **WHEN** an outgoing HTTP request is intercepted
- **AND** the access token is expired (or absent) but a valid refresh token is stored
- **THEN** the interceptor SHALL call `POST /auth/refresh` first
- **AND** on success, SHALL store the new token pair and attach the new `Authorization: Bearer <token>` header to the original request

#### Scenario: Request proceeds normally when access token is valid
- **WHEN** an outgoing HTTP request is intercepted
- **AND** the access token is present and not expired
- **THEN** the interceptor SHALL attach the token and NOT call `POST /auth/refresh`

### Requirement: AuthInterceptor retries a 401 response via token refresh
As a fallback, when a request returns HTTP 401, the `authInterceptor` SHALL attempt one token refresh and retry the original request. If the retry also fails with 401, the user SHALL be redirected to login.

#### Scenario: 401 response triggers refresh and retry
- **WHEN** a request returns HTTP 401
- **AND** a refresh token is available
- **THEN** the interceptor SHALL call `POST /auth/refresh`, store the new tokens, and retry the original request once with the new access token

#### Scenario: Refresh failure on 401 redirects to login
- **WHEN** a request returns HTTP 401
- **AND** the refresh call also fails (401 or network error)
- **THEN** the interceptor SHALL call `authService.clearTokens()` and navigate to `/auth/login`

#### Scenario: 401 with no refresh token redirects to login immediately
- **WHEN** a request returns HTTP 401
- **AND** no refresh token is stored
- **THEN** the interceptor SHALL call `authService.clearTokens()` and navigate to `/auth/login` without attempting a refresh

### Requirement: Concurrent refresh requests are deduplicated
If multiple requests trigger a refresh simultaneously, only one `POST /auth/refresh` call SHALL be made. All pending requests SHALL wait for that single refresh to complete and then retry with the new token.

#### Scenario: Two simultaneous expired-token requests share one refresh call
- **WHEN** two requests are dispatched concurrently and both detect an expired access token
- **THEN** only one `POST /auth/refresh` SHALL be sent
- **AND** both original requests SHALL be retried with the new access token after the refresh completes
