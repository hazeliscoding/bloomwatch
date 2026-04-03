## ADDED Requirements

### Requirement: Login page includes a "Forgot password?" link
The login page (`LoginComponent`) SHALL display a "Forgot password?" link below the login form. Clicking it SHALL navigate to `/auth/forgot-password`.

#### Scenario: Forgot password link is visible on login page
- **WHEN** a guest user views the login page
- **THEN** a "Forgot password?" link SHALL be visible below the form's submit button

#### Scenario: Forgot password link navigates to the forgot-password page
- **WHEN** a user clicks the "Forgot password?" link on the login page
- **THEN** the browser SHALL navigate to `/auth/forgot-password`

---

### Requirement: Forgot-password page allows the user to request a reset link
The system SHALL provide a `ForgotPasswordComponent` at route `/auth/forgot-password` rendered inside `MinimalLayout`. It SHALL display a single email input and a submit button. On submission it SHALL call `AuthService.forgotPassword(email)` and, on any response (success or error), display a confirmation message: "If that email is registered, we've sent a reset link. Check your inbox." The form SHALL be disabled during the in-flight request.

#### Scenario: Valid email submission shows confirmation
- **WHEN** a user enters a valid email address and submits the forgot-password form
- **THEN** `AuthService.forgotPassword(email)` SHALL be called
- **AND** the form SHALL be replaced with a confirmation message regardless of whether the email exists

#### Scenario: Form is disabled during submission
- **WHEN** the forgot-password form has been submitted and the request is pending
- **THEN** the submit button SHALL be in a loading/disabled state and the email input SHALL be read-only

#### Scenario: Invalid email format shows inline validation
- **WHEN** a user submits the forgot-password form with an input that is not a valid email address
- **THEN** an inline validation error SHALL appear below the email field and the request SHALL NOT be sent

#### Scenario: "Back to login" link is present
- **WHEN** a user views the forgot-password page
- **THEN** a "Back to login" link SHALL be visible that navigates to `/auth/login`

---

### Requirement: Reset-password page allows the user to set a new password
The system SHALL provide a `ResetPasswordComponent` at route `/auth/reset-password` rendered inside `MinimalLayout`. It SHALL read the `token` query parameter on init. It SHALL display a new-password input and a confirm-password input. On submission it SHALL call `AuthService.resetPassword(token, newPassword)`. On success it SHALL display a success message and a "Go to login" link. On `400` error it SHALL display the server-provided error message.

#### Scenario: Page with valid token displays the reset form
- **WHEN** a user navigates to `/auth/reset-password?token=<value>`
- **THEN** the reset-password form SHALL be displayed with new-password and confirm-password fields

#### Scenario: Page without token query parameter shows an error state
- **WHEN** a user navigates to `/auth/reset-password` with no `token` query parameter
- **THEN** the component SHALL display an error message: "Invalid or missing reset link." and SHALL NOT show the password form

#### Scenario: Successful reset shows confirmation with login link
- **WHEN** the user submits a valid new password and the API returns HTTP 200
- **THEN** the form SHALL be replaced with "Your password has been reset!" and a "Go to login" link pointing to `/auth/login`

#### Scenario: Expired or used token shows error from API
- **WHEN** the user submits the form and the API returns HTTP 400
- **THEN** the server error message SHALL be displayed below the form (e.g., "Reset link has expired. Please request a new one.")

#### Scenario: Passwords do not match shows inline validation
- **WHEN** the user submits the form with mismatched new-password and confirm-password values
- **THEN** a validation error SHALL appear and the request SHALL NOT be sent

#### Scenario: Form is disabled during submission
- **WHEN** the reset-password form has been submitted and the request is pending
- **THEN** the submit button SHALL be in a loading/disabled state and both password inputs SHALL be read-only

---

### Requirement: AuthService exposes forgotPassword and resetPassword methods
`AuthService` SHALL expose:
- `forgotPassword(email: string): Observable<void>` — calls `POST /auth/forgot-password`
- `resetPassword(token: string, newPassword: string): Observable<void>` — calls `POST /auth/reset-password`

Both methods SHALL delegate to `ApiService` and let error propagation follow the standard Angular HTTP error pipeline (caller handles via `catchError`).

#### Scenario: forgotPassword delegates to ApiService
- **WHEN** `forgotPassword(email)` is called
- **THEN** it SHALL call `ApiService.post('/auth/forgot-password', { email })` and return the resulting Observable

#### Scenario: resetPassword delegates to ApiService
- **WHEN** `resetPassword(token, newPassword)` is called
- **THEN** it SHALL call `ApiService.post('/auth/reset-password', { token, newPassword })` and return the resulting Observable
