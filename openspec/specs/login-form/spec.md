### Requirement: Login form renders email and password fields
The login page SHALL render a form at route `/login` containing two input fields: email and password. Both fields SHALL use the `BloomInputComponent` and SHALL be marked as required.

#### Scenario: Form renders with all fields
- **WHEN** a user navigates to `/login`
- **THEN** the page SHALL display a form with labeled inputs for email and password, a submit button, and a link to the registration page

#### Scenario: Password field uses password type
- **WHEN** the login form renders
- **THEN** the password field SHALL have `type="password"` to mask input

### Requirement: Client-side validation prevents invalid submissions
The form SHALL validate all fields on the client side before allowing submission. Validation errors SHALL display only after the field has been touched (blurred) or after a submit attempt.

#### Scenario: All fields required
- **WHEN** a user attempts to submit the form with any field left empty
- **THEN** each empty field SHALL display an error message indicating it is required, and the form SHALL NOT submit

#### Scenario: Invalid email format
- **WHEN** a user enters a value in the email field that does not match a valid email pattern
- **THEN** the email field SHALL display an error message "Please enter a valid email address"

#### Scenario: Errors only shown after interaction
- **WHEN** a user has not yet blurred a field and has not attempted to submit
- **THEN** no validation errors SHALL be displayed for that field

### Requirement: Form submits login request to backend
The form SHALL submit a `POST /auth/login` request with the email and password when all client-side validations pass.

#### Scenario: Successful form submission
- **WHEN** a user fills in email and password with valid data and clicks the submit button
- **THEN** the form SHALL send `POST /auth/login` with `{ email, password }` via `ApiService`

#### Scenario: Submit button disabled during submission
- **WHEN** a login request is in flight
- **THEN** the submit button SHALL be disabled and SHALL display a loading state

#### Scenario: Submit button disabled when form is invalid
- **WHEN** any client-side validation error exists
- **THEN** the submit button SHALL be disabled

### Requirement: Successful login stores token and redirects
On a successful login response, the system SHALL store the JWT token and redirect the user to their watch spaces.

#### Scenario: Token stored and user redirected
- **WHEN** `POST /auth/login` returns HTTP 200 with `{ accessToken, expiresAt }`
- **THEN** the system SHALL call `AuthService.setToken(accessToken, expiresAt)` and navigate to `/watch-spaces`

### Requirement: Invalid credentials display a generic error message
The form SHALL display a generic error message on authentication failure without revealing whether the email exists in the system.

#### Scenario: Invalid credentials error
- **WHEN** `POST /auth/login` returns HTTP 401 Unauthorized
- **THEN** the form SHALL display a form-level error message "Invalid email or password" and SHALL NOT indicate whether the email exists

#### Scenario: Error clears on new submission
- **WHEN** a user modifies any field after an error was displayed
- **THEN** the form-level error message SHALL be cleared

### Requirement: Unexpected server errors are displayed to the user
The form SHALL handle unexpected server error responses and display an appropriate message.

#### Scenario: Unexpected server error
- **WHEN** `POST /auth/login` returns HTTP 500 or a network error
- **THEN** the form SHALL display a form-level error message "Something went wrong. Please try again."

### Requirement: Login page follows kawaii/Y2K design system
The login page SHALL be styled using the BloomWatch design tokens and component library, consistent with the kawaii/Y2K aesthetic and matching the registration page layout.

#### Scenario: Form uses design tokens exclusively
- **WHEN** the login page renders
- **THEN** all colors, spacing, typography, border-radius, and shadows SHALL reference `--bloom-*` CSS custom properties with no hardcoded values

#### Scenario: Responsive layout
- **WHEN** the viewport width is less than 640px
- **THEN** the form layout SHALL adapt to a single-column, full-width layout with appropriate padding

#### Scenario: Light and dark theme support
- **WHEN** the user switches between light and dark themes
- **THEN** the login page SHALL correctly render using the active theme's semantic tokens

### Requirement: Login form is accessible
The login form SHALL meet WCAG AA accessibility standards.

#### Scenario: Form fields have associated labels
- **WHEN** a screen reader focuses on an input field
- **THEN** the field's label SHALL be announced, and required fields SHALL be indicated

#### Scenario: Error messages are announced
- **WHEN** a validation error appears on a field
- **THEN** the error message SHALL be associated via `aria-describedby` and announced with `role="alert"`

#### Scenario: Form is keyboard navigable
- **WHEN** a user navigates the form using only the keyboard
- **THEN** all fields and the submit button SHALL be reachable via Tab, and the form SHALL be submittable via Enter
