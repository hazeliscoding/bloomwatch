## Why

Story 10.1 introduces the Watch Space Dashboard as the default landing page when entering a watch space (`/watch-spaces/:id`). Currently, navigating to a watch space drops the user directly into the anime list and settings view, with no high-level summary of activity. Members need an at-a-glance overview of their shared watching progress, compatibility, and backlog before diving into individual titles. The backend endpoint (`GET /watchspaces/{id}/dashboard`) is fully implemented (Epic 5); this change builds the frontend page to consume it.

## What Changes

- **New dashboard page component** that fetches `GET /watchspaces/{id}/dashboard` and renders:
  - Stat card row: Total Shows, Currently Watching, Finished, Episodes Watched Together
  - Compatibility score section with circular ring, label, and rated-together context (graceful "Not enough ratings yet" when null)
  - Currently Watching grid: up to 5 anime with cover, title, and progress bar
  - Backlog Highlights grid: up to 5 anime with cover, title, and mood/vibe/pitch tags
  - Rating Gap Highlights: up to 3 anime with per-member scores and delta display
- **New TypeScript interfaces** for the dashboard API response shape (stats, compatibility, currently watching items, backlog items, rating gap items)
- **New service method** `getDashboard(spaceId)` on `WatchSpaceService`
- **Route restructuring**: `/watch-spaces/:id` becomes the dashboard; the existing anime list + settings view moves to `/watch-spaces/:id/manage` (or similar child route)
- **Loading skeleton and error states** for the dashboard page
- **Navigation links** between dashboard and anime list/settings

## Capabilities

### New Capabilities
- `watch-space-dashboard`: Frontend dashboard page that renders the watch space summary, stat cards, compatibility ring, currently watching grid, backlog highlights, and rating gap highlights with loading/error/empty states

### Modified Capabilities
_(No existing spec-level requirements are changing. The route restructuring is an implementation concern handled in design/tasks.)_

## Impact

- **Routing**: `watch-spaces.routes.ts` — `:id` path changes from `WatchSpaceDetail` to the new dashboard component; existing detail/manage view gets a new child route
- **Models**: `watch-space.model.ts` — new interfaces for dashboard response types
- **Service**: `watch-space.service.ts` — new `getDashboard()` method
- **New files**: dashboard component (`.ts`, `.html`, `.scss`), dashboard spec tests (`.spec.ts`)
- **Existing components**: `WatchSpaceDetail` template needs a navigation link back to dashboard; dashboard needs links to anime list and settings
- **No backend changes** — the `GET /watchspaces/{id}/dashboard` endpoint is already complete
