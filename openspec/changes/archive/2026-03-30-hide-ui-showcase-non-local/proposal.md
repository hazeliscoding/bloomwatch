## Why

The UI Showcase route (`/showcase`) is a developer tool for previewing shared UI components during local development. It should not be accessible in production or staging environments since it exposes internal component implementations and has no value for end-users. Currently it is always registered in the Angular router regardless of environment.

## What Changes

- Conditionally register the `/showcase` route only when the Angular app is running in development mode (`environment.production === false`).
- Remove or hide any navigation links pointing to the showcase when not in development mode.
- No user-facing functionality is affected — the showcase is purely a developer utility.

## Capabilities

### New Capabilities

- `dev-only-showcase-route`: Conditionally include the showcase route and any associated navigation links based on the runtime environment flag.

### Modified Capabilities

_(none — no existing spec-level requirements are changing)_

## Impact

- **Routing**: `app.routes.ts` — the showcase route will be conditionally included.
- **Navigation**: Any sidebar or nav links referencing `/showcase` will be conditionally rendered.
- **Environment config**: Relies on the existing `environment.production` flag; no new environment variables needed.
- **Bundle size**: In a production build the showcase module will not be loaded (already lazy-loaded, but route removal ensures it is completely unreachable).
