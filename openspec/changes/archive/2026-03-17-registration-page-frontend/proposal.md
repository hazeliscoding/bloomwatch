## Why

The backend registration endpoint (`POST /auth/register`) is complete, but the Angular frontend still shows a placeholder at `/register`. Users have no way to create a BloomWatch account through the UI. This is the next step in Epic 7 (Auth Frontend) after the completed landing page, and it unblocks login page and auth guard work.

## What Changes

- Replace the placeholder register component with a fully functional registration form (display name, email, password, confirm password)
- Add client-side validation (required fields, email format, password min length, password match)
- Wire form submission to `POST /auth/register` via `ApiService`
- Auto-login on success (call `POST /auth/login`, store token) and redirect to `/watch-spaces`
- Display server-side errors inline (e.g. duplicate email) or as form-level alerts
- Add loading state during API calls
- Style with kawaii/Y2K design system tokens and existing bloom-* components

## Capabilities

### New Capabilities
- `registration-form`: Client-side registration form with validation, API integration, auto-login flow, error handling, and kawaii/Y2K styling

### Modified Capabilities

_(none — no existing spec-level requirements are changing)_

## Impact

- **Components modified:** `features/auth/register.ts` (replace placeholder with full implementation), new `register.scss`
- **Services used:** `AuthService` (token storage), `ApiService` (HTTP calls)
- **Dependencies:** existing bloom-input, bloom-button components; existing auth interceptor and routing
- **No new dependencies or breaking changes**
