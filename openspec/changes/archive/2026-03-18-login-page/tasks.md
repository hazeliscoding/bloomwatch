## 1. Component Setup

- [x] 1.1 Replace placeholder `login.ts` with full component scaffold: imports, signal-based form state (email, password, touchedFields, submitAttempted, isSubmitting, formError), and empty template
- [x] 1.2 Create `login.scss` with card layout, header, error banner, fields, submit, and footer styles mirroring `register.scss` (using `--bloom-*` tokens exclusively)

## 2. Template and Validation

- [x] 2.1 Build the login form template: header ("Welcome Back" title, subtitle), email `BloomInputComponent` (type=email, autocomplete=email), password `BloomInputComponent` (type=password, autocomplete=current-password), submit `BloomButtonComponent`, and footer link to `/register`
- [x] 2.2 Implement client-side validation computeds: emailError (required + format), passwordError (required), isValid, and isFieldVisible (touched or submitAttempted)

## 3. Form Submission and API Integration

- [x] 3.1 Implement `onSubmit()`: prevent default, set submitAttempted, clear formError, guard on isValid, set isSubmitting, call `POST /auth/login` via `ApiService`
- [x] 3.2 Handle success: call `AuthService.setToken(accessToken, expiresAt)`, navigate to `/watch-spaces`
- [x] 3.3 Handle errors: 401 → set formError to "Invalid email or password"; 500/network → set formError to "Something went wrong. Please try again."; reset isSubmitting
- [x] 3.4 Clear formError when user modifies email or password fields

## 4. Accessibility and Responsive

- [x] 4.1 Verify form-level error banner has `role="alert"`, fields use `aria-describedby` for errors (handled by BloomInputComponent), and form is keyboard-navigable with Enter submission
- [x] 4.2 Add responsive styles for mobile (< 640px) using `bp-mobile-only` mixin: reduced padding, smaller title font

## 5. Testing

- [x] 5.1 Write component tests: form renders with email/password fields and submit button, validation errors show after touch/submit, submit disabled when invalid or submitting, successful login stores token and redirects, 401 shows generic error, 500 shows server error banner, error clears on field change
