## 1. Component Scaffold & Styling

- [x] 1.1 Create `register.scss` with form layout, card container, responsive breakpoint (< 640px), and kawaii/Y2K styling using `--bloom-*` tokens exclusively
- [x] 1.2 Replace placeholder `register.ts` with full component scaffold: imports (BloomInputComponent, BloomButtonComponent, RouterLink), signals for form fields, inline template with form structure, and `styleUrl` pointing to `register.scss`

## 2. Client-Side Validation

- [x] 2.1 Add signal-based validation: `computed` error signals for each field (displayName required, email required + format, password required + min 8 chars, confirmPassword required + must match password)
- [x] 2.2 Add touched tracking (per-field `Set<string>` signal + `submitAttempted` signal) so errors only display after blur or submit attempt
- [x] 2.3 Wire `[error]` inputs on each `BloomInputComponent` to show computed errors only when touched/submitted

## 3. Form Submission & API Integration

- [x] 3.1 Add `isValid` computed signal gating the submit button disabled state; add `isSubmitting` signal for loading state
- [x] 3.2 Implement `onSubmit()`: mark all fields touched, guard on validity, set `isSubmitting`, call `ApiService.post('/auth/register', { email, password, displayName })`
- [x] 3.3 Implement auto-login flow: on 201 success, call `ApiService.post('/auth/login', { email, password })`, store token via `AuthService.setToken()`, navigate to `/watch-spaces`

## 4. Error Handling

- [x] 4.1 Handle 409 Conflict: set server-side email error signal ("This email is already registered")
- [x] 4.2 Handle 400 Bad Request: parse validation errors from response body and map to field-level error signals
- [x] 4.3 Handle 500/network errors: set form-level error signal ("Something went wrong. Please try again.") displayed as a banner above the form
- [x] 4.4 Handle auto-login failure: show success message and redirect to `/login`

## 5. Accessibility & Polish

- [x] 5.1 Ensure all inputs have proper labels, `required` attributes, `autocomplete` hints (name, email, new-password), and `aria-describedby` linkage via BloomInputComponent
- [x] 5.2 Ensure form is keyboard-navigable (Tab order, Enter to submit) and error messages use `role="alert"`
- [x] 5.3 Verify light/dark theme rendering using semantic tokens

## 6. Testing

- [x] 6.1 Write unit tests for the Register component: field rendering, validation logic (required, email format, password length, password match), touched behavior
- [x] 6.2 Write unit tests for submission flow: successful register + auto-login, 409 error handling, 400 error handling, 500/network error handling, auto-login failure fallback
