## Context

An Angular 21 project has been bootstrapped at `src/BloomWatch.UI/` with standalone components, SCSS, Vitest, and `provideRouter` already wired in `app.config.ts`. The routes array is empty and `app.html` contains the default Angular placeholder. This design covers Story 6.1: adding the directory structure, routing, and layouts on top of the existing scaffold.

Angular 21 uses simplified file naming conventions: `app.ts` instead of `app.component.ts`, `app.html` instead of `app.component.html`. New components will follow this convention.

## Goals / Non-Goals

**Goals:**
- Establish the feature-based directory structure (`core/`, `shared/`, `features/`) from the architecture doc
- Configure lazy-loaded routes for all MVP pages
- Implement a dual-layout system: authenticated shell (nav bar + content) and public minimal layout
- Replace the default placeholder template with a working routing shell
- All new components use Angular 21 standalone conventions

**Non-Goals:**
- HTTP client / auth interceptor setup (Story 6.2)
- Theme system / dark mode (Story 6.3)
- Auth guards or token management (Story 7.4)
- Any actual page content — feature routes render placeholder components
- Backend changes or CORS configuration

## Decisions

### 1. Angular 21 file naming convention

Follow Angular 21's simplified naming: `dashboard.ts` not `dashboard.component.ts`, `shell-layout.html` not `shell-layout.component.html`. This matches the existing scaffold's convention (`app.ts`, `app.html`, `app.scss`).

### 2. Lazy loading via `loadChildren` route configs

Each feature area gets its own route file (e.g., `features/auth/auth.routes.ts`) loaded lazily from `app.routes.ts`. This keeps the initial bundle small.

```
app.routes.ts
  ShellLayoutComponent children (lazy):
    '/'              → features/dashboard/dashboard.routes.ts
    '/watch-spaces'  → features/watch-spaces/watch-spaces.routes.ts
    '/settings'      → features/settings/settings.routes.ts
  MinimalLayoutComponent children (lazy):
    '/login'         → features/auth/ (login component)
    '/register'      → features/auth/ (register component)
```

### 3. Dual-layout system via route nesting

Use route-level layout components rather than conditional logic:

- **Authenticated routes** (`/`, `/watch-spaces/**`, `/settings`) nest under `ShellLayoutComponent` with nav bar + `<router-outlet>`.
- **Public routes** (`/login`, `/register`) nest under `MinimalLayoutComponent` with only `<router-outlet>`.

This makes it trivial to add auth guards to the shell parent route in Story 7.4.

**Alternative considered:** Single layout with conditional nav bar. Rejected because route-based layouts are more composable and easier to guard.

### 4. Directory structure

```
src/BloomWatch.UI/src/app/
  core/
    layout/
      shell-layout/       # Nav bar + content (authenticated)
      minimal-layout/     # No nav bar (public)
  shared/
    ui/                   # Reusable UI components (empty for now)
    models/               # Shared TypeScript interfaces (empty for now)
  features/
    auth/                 # Login + register routes
    dashboard/            # Main dashboard route
    watch-spaces/         # Space list, detail, anime detail routes
    settings/             # Settings route
  app.routes.ts           # Top-level route config with lazy loading
  app.ts                  # Root component (already exists)
  app.config.ts           # Application config (already exists, provideRouter wired)
```

### 5. Root component simplification

Replace the default Angular placeholder in `app.html` with just `<router-outlet />`. The root `app.ts` component already imports `RouterOutlet` — no changes needed there.

## Risks / Trade-offs

- **[Placeholder components are throwaway]** → Acceptable. Each feature story replaces the placeholder. The routing and layout structure persists.
- **[No auth guards yet]** → Authenticated routes are accessible without login. Story 7.4 adds guards to the shell route. Intentional — guards depend on Story 6.2's auth service.
- **[CORS not configured]** → No API calls in this story, so not a blocker. Handled in Story 6.2.
