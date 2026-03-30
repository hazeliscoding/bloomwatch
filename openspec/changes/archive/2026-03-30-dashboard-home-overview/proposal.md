## Why

The shell nav's "Dashboard" button currently routes to a bare placeholder component (`<h1>Dashboard</h1><p>Welcome to BloomWatch.</p>`) at the root authenticated path `/`. Users who land here after login get no meaningful overview of their activity. Meanwhile, the detailed per-watch-space dashboard already exists as a separate spec (`watch-space-dashboard`). The top-level route needs to become a proper **Home** page that gives users an at-a-glance overview of their BloomWatch world — their watch spaces, recent activity, and quick shortcuts — before they dive into any specific watch space.

## What Changes

- **Rename the shell nav item** from "Dashboard" to "Home" and keep it pointing to `/`.
- **Replace the placeholder Dashboard component** with a full Home page that shows:
  - A personalised greeting section (e.g., "Welcome back, Hazel").
  - A summary strip of global stats across all watch spaces (total watch spaces, total anime tracked, total episodes watched together).
  - A "Your Watch Spaces" card grid showing each watch space the user belongs to, with member avatars, quick stats (watching count, backlog count), and a link to the per-watch-space dashboard.
  - A "Recently Active" section highlighting the 3 most recently updated anime across all watch spaces (cover, title, watch space name, last activity).
  - Quick-action buttons: "Create Watch Space" and "Browse Anime".
- **Add a new API endpoint** `GET /home/overview` that aggregates cross-watch-space summary data for the authenticated user.
- **Empty state handling**: When the user has no watch spaces, show a friendly onboarding prompt with a CTA to create their first watch space.

## Capabilities

### New Capabilities
- `home-overview`: Top-level home page UI component with greeting, global stats, watch space cards, recent activity, and quick actions. Includes the `GET /home/overview` backend endpoint.

### Modified Capabilities
_(none — the per-watch-space dashboard remains unchanged; we are only replacing the top-level placeholder)_

## Impact

- **Frontend**: Replaces `src/app/features/dashboard/dashboard.ts` with a full Home component. Updates `dashboard.routes.ts`. Updates shell nav label from "Dashboard" to "Home".
- **Backend**: New `GET /home/overview` endpoint in the API layer, aggregating data from WatchSpaces and AnimeTracking modules.
- **Routing**: No route path changes — `/` within the shell layout continues to load the dashboard feature module; only the component content changes.
- **Existing specs**: No spec-level requirement changes to `watch-space-dashboard`, `dashboard-summary`, or any other existing spec.
