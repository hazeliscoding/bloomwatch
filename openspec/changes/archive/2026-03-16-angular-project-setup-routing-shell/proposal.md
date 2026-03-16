## Why

BloomWatch has a complete backend for Identity and WatchSpaces modules but no frontend beyond a bootstrapped Angular 21 scaffold. All user-facing epics (6-10) are blocked until the Angular shell has routing, layouts, and feature structure in place. Story 6.1 builds on the existing scaffold at `src/BloomWatch.UI/` to establish the directory structure, lazy-loaded routes, and dual-layout system that every future frontend feature depends on.

## What Changes

- Restructure the existing Angular 21 project at `src/BloomWatch.UI/` into a feature-based directory layout (`core/`, `shared/`, `features/`)
- Replace the default Angular placeholder template with a clean `<router-outlet />`
- Configure lazy-loaded routes for all MVP pages: `/`, `/login`, `/register`, `/watch-spaces`, `/watch-spaces/:id`, `/watch-spaces/:id/anime/:animeId`, `/settings`
- Create a `ShellLayoutComponent` (nav bar + content area) for authenticated routes
- Create a `MinimalLayoutComponent` (no nav bar) for public routes (landing, login, register)
- Create placeholder components for each feature area

## Capabilities

### New Capabilities
- `angular-routing-shell`: Feature-based directory structure, lazy-loaded routing, and dual-layout system (authenticated shell vs. public minimal) built on the existing Angular 21 scaffold

### Modified Capabilities

_None — this is the first frontend change._

## Impact

- **Modified code:** `src/BloomWatch.UI/src/app/` — new directories, components, and route config
- **Files replaced:** `app.html` (default placeholder removed), `app.routes.ts` (populated with lazy routes)
- **No new dependencies** — uses Angular Router already present in `package.json`
- **No backend changes** — frontend-only
