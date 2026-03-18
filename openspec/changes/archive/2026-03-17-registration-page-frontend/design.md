## Context

The backend registration (`POST /auth/register`) and login (`POST /auth/login`) endpoints are complete. The Angular frontend has a placeholder `Register` component at `/register` under `MinimalLayout` (no nav). The component library provides `BloomInputComponent` (with ControlValueAccessor, validation states, error display) and `BloomButtonComponent` (with loading state, gel styling). `AuthService` handles token persistence via signals. `ApiService` wraps HttpClient for all API calls.

## Goals / Non-Goals

**Goals:**
- Functional registration form with client-side validation and server error handling
- Auto-login after successful registration (call login endpoint, store token, redirect)
- Consistent kawaii/Y2K styling using existing design tokens and bloom-* components
- Accessible form with proper ARIA attributes, keyboard navigation, and screen reader support
- Responsive layout that works on mobile (< 640px) and desktop

**Non-Goals:**
- OAuth / social login (future epic)
- Email verification flow (backend sets `isEmailVerified = false`; verification UI is out of scope)
- Auth route guards redirecting authenticated users away (Story 7.4, separate change)
- Password strength meter or complexity requirements beyond 8-char minimum
- Captcha or rate limiting on the frontend

## Decisions

### 1. Signal-based reactive form (no Angular Reactive Forms module)

Use Angular signals (`signal`, `computed`) for form state and validation rather than importing `ReactiveFormsModule` or `FormsModule`. Each field gets a `signal<string>` for its value and a `computed<string>` for its error message. A `touched` signal set tracks which fields have been blurred (errors only show after touch or submit attempt).

**Why:** Keeps the component standalone with zero extra module imports. Aligns with the signal-based pattern used by `AuthService` and `BloomInputComponent`. Simpler mental model — no FormGroup/FormControl abstraction layer.

**Alternative considered:** Angular Reactive Forms — adds module dependency, heavier API surface, not needed for a 4-field form.

### 2. BloomInputComponent via value/event binding (not ControlValueAccessor)

Bind to `BloomInputComponent` using `[value]` and `(valueChange)` event bindings with explicit signal updates, rather than `ngModel` or `formControl` directives.

**Why:** Avoids importing `FormsModule`/`ReactiveFormsModule`. The `valueChange` output already emits on every keystroke. Error messages are passed via the `[error]` input. This is the lightest integration path.

### 3. Auto-login flow: register → login → redirect

After successful `POST /auth/register`, immediately call `POST /auth/login` with the same credentials. On login success, store the token via `AuthService.setToken()` and navigate to `/watch-spaces`.

**Why:** The register endpoint returns `{ id, email, displayName }` but no token. A separate login call is needed to obtain a JWT. This matches the backend API contract. The user doesn't need to re-enter credentials — we already have them in memory.

**Alternative considered:** Having the register endpoint return a token — would require backend changes, out of scope.

### 4. Error mapping strategy

Map HTTP error responses to user-friendly inline messages:
- **409 Conflict** → show "This email is already registered" on the email field
- **400 Bad Request** → parse validation errors from response body and map to fields
- **Network/5xx errors** → show a form-level error banner ("Something went wrong. Please try again.")

**Why:** Inline field errors give the best UX for field-specific issues. A form-level banner catches unexpected errors without confusing the user about which field is wrong.

### 5. Component file structure

Replace the placeholder `register.ts` in-place. Add a companion `register.scss` file in the same directory (`features/auth/`). No new services or models — types are defined inline in the component file.

**Why:** Mirrors the landing page pattern (`landing.ts` + `landing.scss`). Keeps auth feature files co-located. A 4-field form doesn't warrant a separate service.

## Risks / Trade-offs

- **Auto-login adds a second network call** → Acceptable latency; the loading state covers both calls. If the login call fails after registration succeeds, show a success message and redirect to `/login` so the user can log in manually.
- **No auth guard yet (Story 7.4)** → Authenticated users can visit `/register`. This is harmless — they just see the form. Guard will be added in a separate story.
- **Signal-based validation is manual** → No built-in async validator pipeline. Acceptable for 4 fields; keeps things simple. If validation grows complex, can migrate to Reactive Forms later.
