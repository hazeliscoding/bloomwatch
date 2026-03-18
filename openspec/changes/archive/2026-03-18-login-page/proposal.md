## Why

The backend login endpoint (`POST /auth/login`) is complete and the Angular frontend has a placeholder at `/login`, but users cannot actually sign in through the UI. This is the next step in Epic 7 (Auth Frontend) after the completed registration page (Story 7.2), and it unblocks auth route guards (Story 7.4) and all authenticated frontend work.

## What Changes

- Replace the placeholder login component with a fully functional login form (email, password)
- Add client-side validation (required fields, email format)
- Wire form submission to `POST /auth/login` via `ApiService`
- On success, store the JWT via `AuthService.setToken()` and redirect to `/watch-spaces`
- Display a generic error message on invalid credentials without revealing whether the email exists (prevents user enumeration)
- Display a form-level error banner for network/server errors
- Add loading state during the API call
- Style with kawaii/Y2K design system tokens and existing bloom-* components
- Include a link to the registration page for new users

## Capabilities

### New Capabilities
- `login-form`: Client-side login form with email/password fields, validation, API integration, token storage, error handling, and kawaii/Y2K styling

### Modified Capabilities

_(none -- no existing spec-level requirements are changing)_

## Impact

- **Components modified:** `features/auth/login.ts` (replace placeholder with full implementation), new `login.scss`
- **Services used:** `AuthService` (token storage via `setToken()`), `ApiService` (HTTP calls to `POST /auth/login`)
- **Dependencies:** existing `BloomInputComponent`, `BloomButtonComponent`; existing auth interceptor and routing
- **No new dependencies or breaking changes**
