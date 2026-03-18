## Context

The backend login endpoint (`POST /auth/login`) is complete and returns `{ accessToken, expiresAt }` on success, or HTTP 401 for invalid credentials (generic message, no user enumeration). The Angular frontend has a placeholder `Login` component at `/login` under `MinimalLayout`. The registration page (Story 7.2) is complete and establishes the pattern for auth form pages — signal-based state, `BloomInputComponent`/`BloomButtonComponent`, `ApiService` for HTTP, `AuthService` for token storage, kawaii/Y2K styling with `--bloom-*` tokens.

## Goals / Non-Goals

**Goals:**
- Functional login form with email and password fields and client-side validation
- Submit credentials to `POST /auth/login`, store JWT on success, redirect to `/watch-spaces`
- Generic error message on 401 (no user enumeration leaks)
- Form-level error banner for network/server errors
- Consistent kawaii/Y2K styling matching the registration page
- Accessible form with proper ARIA attributes, keyboard navigation, and screen reader support
- Responsive layout (mobile < 640px and desktop)

**Non-Goals:**
- "Remember me" or persistent sessions (token expires after 1 hour per spec)
- Password reset / forgot password flow (future work)
- OAuth / social login (future epic)
- Auth route guards redirecting authenticated users away (Story 7.4, separate change)
- Rate limiting or brute-force protection on the frontend

## Decisions

### 1. Signal-based reactive form (same pattern as registration)

Use Angular signals (`signal`, `computed`) for form state and validation, matching the registration page pattern. Each field gets a `signal<string>` for its value and a `computed<string>` for its error. A `touched` signal set tracks which fields have been blurred.

**Why:** Consistency with the registration form. No extra module imports (`ReactiveFormsModule`/`FormsModule`). Simpler for a 2-field form.

**Alternative considered:** Angular Reactive Forms — unnecessary overhead for two fields.

### 2. BloomInputComponent via value/event binding

Bind to `BloomInputComponent` using `(valueChange)` with explicit signal updates and `[error]` for validation messages, same as registration.

**Why:** Avoids `FormsModule` imports. Consistent with the established pattern.

### 3. Single generic error message for 401

Display "Invalid email or password" for all 401 responses, regardless of whether the email exists or the password is wrong.

**Why:** The backend already returns a generic 401 for both cases (per user-authentication spec). The frontend mirrors this to prevent user enumeration at the UI level.

### 4. Component file structure mirrors registration

Replace the placeholder `login.ts` in-place. Add a companion `login.scss` in the same `features/auth/` directory. Reuse the same card layout, error banner, and footer link patterns from registration.

**Why:** Visual and structural consistency across auth pages. Users see a cohesive experience.

### 5. LoginResponse interface defined inline

Define the `LoginResponse` interface (`{ accessToken: string; expiresAt: string }`) in the component file, same as registration.

**Why:** A 2-field form with one API call doesn't warrant a separate types file.

## Risks / Trade-offs

- **No auth guard yet (Story 7.4)** → Authenticated users can visit `/login`. Harmless — they just see the form. Guard will be added in a separate story.
- **No "forgot password" link** → Users who forget their password have no recovery path yet. Acceptable for MVP — this will be added in a future story.
- **Single error message for all 401s** → Users can't distinguish between wrong email and wrong password. This is intentional for security but may cause UX friction. Mitigated by the clear error text.
