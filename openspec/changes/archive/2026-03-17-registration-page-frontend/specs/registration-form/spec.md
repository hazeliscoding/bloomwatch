## ADDED Requirements

### Requirement: Registration form renders all required fields
The registration page SHALL render a form at route `/register` containing four input fields: display name, email, password, and confirm password. All fields SHALL use the `BloomInputComponent` and SHALL be marked as required.

#### Scenario: Form renders with all fields
- **WHEN** a user navigates to `/register`
- **THEN** the page SHALL display a form with labeled inputs for display name, email, password, and confirm password, a submit button, and a link to the login page

#### Scenario: Password fields use password type
- **WHEN** the registration form renders
- **THEN** the password and confirm password fields SHALL have `type="password"` to mask input

### Requirement: Client-side validation prevents invalid submissions
The form SHALL validate all fields on the client side before allowing submission. Validation errors SHALL display only after the field has been touched (blurred) or after a submit attempt.

#### Scenario: All fields required
- **WHEN** a user attempts to submit the form with any field left empty
- **THEN** each empty field SHALL display an error message indicating it is required, and the form SHALL NOT submit

#### Scenario: Invalid email format
- **WHEN** a user enters a value in the email field that does not match a valid email pattern
- **THEN** the email field SHALL display an error message "Please enter a valid email address"

#### Scenario: Password too short
- **WHEN** a user enters a password shorter than 8 characters
- **THEN** the password field SHALL display an error message "Password must be at least 8 characters"

#### Scenario: Passwords do not match
- **WHEN** the confirm password field value does not match the password field value
- **THEN** the confirm password field SHALL display an error message "Passwords do not match"

#### Scenario: Errors only shown after interaction
- **WHEN** a user has not yet blurred a field and has not attempted to submit
- **THEN** no validation errors SHALL be displayed for that field

### Requirement: Form submits registration request to backend
The form SHALL submit a `POST /auth/register` request with the display name, email, and password when all client-side validations pass.

#### Scenario: Successful form submission
- **WHEN** a user fills in all fields with valid data and clicks the submit button
- **THEN** the form SHALL send `POST /auth/register` with `{ email, password, displayName }` via `ApiService`

#### Scenario: Submit button disabled during submission
- **WHEN** a registration request is in flight
- **THEN** the submit button SHALL be disabled and SHALL display a loading state

#### Scenario: Submit button disabled when form is invalid
- **WHEN** any client-side validation error exists
- **THEN** the submit button SHALL be disabled

### Requirement: Auto-login after successful registration
After a successful registration, the system SHALL automatically log the user in by calling `POST /auth/login` with the same credentials, storing the returned JWT token, and redirecting to `/watch-spaces`.

#### Scenario: Successful registration triggers auto-login
- **WHEN** `POST /auth/register` returns HTTP 201
- **THEN** the system SHALL call `POST /auth/login` with the same email and password, store the returned `accessToken` and `expiresAt` via `AuthService.setToken()`, and navigate to `/watch-spaces`

#### Scenario: Auto-login fails after successful registration
- **WHEN** `POST /auth/register` returns HTTP 201 but the subsequent `POST /auth/login` call fails
- **THEN** the system SHALL display a success message ("Account created! Please log in.") and navigate to `/login`

### Requirement: Server-side errors are displayed to the user
The form SHALL handle server error responses and display appropriate messages to the user.

#### Scenario: Duplicate email error
- **WHEN** `POST /auth/register` returns HTTP 409 Conflict
- **THEN** the email field SHALL display an error message "This email is already registered"

#### Scenario: Server validation error
- **WHEN** `POST /auth/register` returns HTTP 400 with validation errors
- **THEN** the form SHALL map server validation errors to the corresponding fields and display them inline

#### Scenario: Unexpected server error
- **WHEN** `POST /auth/register` returns HTTP 500 or a network error
- **THEN** the form SHALL display a form-level error message "Something went wrong. Please try again."

### Requirement: Registration page follows kawaii/Y2K design system
The registration page SHALL be styled using the BloomWatch design tokens and component library, consistent with the kawaii/Y2K aesthetic.

#### Scenario: Form uses design tokens exclusively
- **WHEN** the registration page renders
- **THEN** all colors, spacing, typography, border-radius, and shadows SHALL reference `--bloom-*` CSS custom properties with no hardcoded values

#### Scenario: Responsive layout
- **WHEN** the viewport width is less than 640px
- **THEN** the form layout SHALL adapt to a single-column, full-width layout with appropriate padding

#### Scenario: Light and dark theme support
- **WHEN** the user switches between light and dark themes
- **THEN** the registration page SHALL correctly render using the active theme's semantic tokens

### Requirement: Registration form is accessible
The registration form SHALL meet WCAG AA accessibility standards.

#### Scenario: Form fields have associated labels
- **WHEN** a screen reader focuses on an input field
- **THEN** the field's label SHALL be announced, and required fields SHALL be indicated

#### Scenario: Error messages are announced
- **WHEN** a validation error appears on a field
- **THEN** the error message SHALL be associated via `aria-describedby` and announced with `role="alert"`

#### Scenario: Form is keyboard navigable
- **WHEN** a user navigates the form using only the keyboard
- **THEN** all fields and the submit button SHALL be reachable via Tab, and the form SHALL be submittable via Enter
