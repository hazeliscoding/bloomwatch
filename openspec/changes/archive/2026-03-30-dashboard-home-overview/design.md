## Context

BloomWatch is a modular monolith (Identity, WatchSpaces, AnimeTracking, Analytics, AniListSync) with an Angular 19+ SPA frontend. The authenticated shell layout has a nav bar with a "Dashboard" link pointing to `/`, which currently renders a placeholder component. The per-watch-space dashboard already exists at `/watch-spaces/:id/dashboard` (spec: `watch-space-dashboard`). Users need a top-level **Home** page that provides a cross-watch-space overview immediately after login.

The frontend uses standalone Angular components with signal-based inputs/outputs, lazy-loaded routes, and the BloomWatch design system (kawaii/Y2K aesthetic with `bloom-*` tokens and BEM SCSS). The backend uses Minimal API endpoints routed through module-specific groups, with application-layer query/command handlers.

## Goals / Non-Goals

**Goals:**
- Provide an at-a-glance overview when the user arrives at `/` after authentication.
- Show a personalised greeting using the user's display name.
- Display aggregated stats across all the user's watch spaces (total watch spaces, total anime, total episodes watched together).
- Show the user's watch spaces as a card grid with member avatars, quick stats, and a link to each watch space's dashboard.
- Highlight the 3 most recently updated anime across all watch spaces for a "recent activity" feel.
- Handle empty state gracefully with an onboarding prompt when the user has no watch spaces yet.
- Rename the shell nav item from "Dashboard" to "Home".

**Non-Goals:**
- Building a real-time activity feed or notification system. "Recent activity" is based on a simple query, not a streaming/event model.
- Changing the per-watch-space dashboard (`watch-space-dashboard` spec) or its API (`GET /watchspaces/{id}/dashboard`).
- Adding new navigation routes. The route path `/` remains the same; only the component content changes.
- Social features, friend lists, or cross-user activity. This is scoped to the authenticated user's own watch spaces.
- Analytics or charting on the home page. The existing Analytics feature covers that.

## Decisions

### 1. Single new API endpoint: `GET /home/overview`

**Decision**: Introduce one new read endpoint that aggregates summary data for the authenticated user across all their watch spaces.

**Rationale**: The frontend needs data from multiple modules (Identity for display name, WatchSpaces for space list + member counts, AnimeTracking for anime stats and recent activity). Making 3+ separate API calls from the frontend adds complexity and latency. A single composite endpoint keeps the client simple and allows server-side optimisation.

**Response shape**:
```json
{
  "displayName": "Hazel",
  "stats": {
    "watchSpaceCount": 3,
    "totalAnimeTracked": 42,
    "totalEpisodesWatchedTogether": 584
  },
  "watchSpaces": [
    {
      "watchSpaceId": "...",
      "name": "Anime Night",
      "role": "Owner",
      "memberCount": 2,
      "memberPreviews": [{ "displayName": "Hazel" }, { "displayName": "Sakura" }],
      "watchingCount": 3,
      "backlogCount": 8
    }
  ],
  "recentActivity": [
    {
      "watchSpaceAnimeId": "...",
      "watchSpaceId": "...",
      "watchSpaceName": "Anime Night",
      "preferredTitle": "Frieren: Beyond Journey's End",
      "coverImageUrl": "https://...",
      "sharedStatus": "Watching",
      "lastUpdatedAt": "2026-03-29T18:00:00Z"
    }
  ]
}
```

**Alternatives considered**:
- *Multiple client-side calls*: Rejected — would require 3+ round trips (`/users/me`, `/watchspaces`, plus per-space anime queries) and complex frontend orchestration.
- *GraphQL*: Rejected — the project uses REST Minimal APIs consistently; introducing GraphQL for one endpoint is over-engineering.

### 2. Endpoint lives in a new `Home` API module (thin orchestrator)

**Decision**: Create a lightweight `BloomWatch.Api.Modules.Home/HomeEndpoints.cs` file that calls into existing application-layer handlers from Identity, WatchSpaces, and AnimeTracking modules. No new domain module is needed — this is a read-only aggregation layer.

**Rationale**: The data already exists across modules. The Home endpoint is a cross-cutting read concern, not a new domain concept. Keeping it as a thin API-layer orchestrator avoids polluting module boundaries.

**Alternatives considered**:
- *Put it in the WatchSpaces module*: Rejected — it queries Identity and AnimeTracking data too; doesn't belong to one module.
- *Create a full `BloomWatch.Modules.Home` domain module*: Rejected — no new domain logic needed, only query aggregation.

### 3. Frontend: Replace the placeholder `Dashboard` component in-place

**Decision**: Replace the content of `src/app/features/dashboard/` with the new Home page. Rename the feature directory from `dashboard` to `home` and update the route import in `app.routes.ts`.

**Rationale**: The existing dashboard feature is a single-file placeholder with no meaningful code to preserve. A clean rename makes the codebase intent clearer. The route path (`''` within the shell) stays the same, so no user-facing URL changes.

### 4. Shell nav label: "Dashboard" → "Home"

**Decision**: Update the nav link text and icon in `shell-layout.html` from "Dashboard" to "Home" with a house icon (★ → ⌂ or similar Unicode/SVG).

**Rationale**: "Home" more accurately describes a cross-cutting overview page. "Dashboard" implies detailed analytics, which is what the per-watch-space dashboard provides.

### 5. Home page sections and layout

**Decision**: The Home page will use a vertical stack layout with these sections:

1. **Greeting header** — "Welcome back, {displayName}" with the display font and gradient text.
2. **Stats strip** — 3 stat cards in a responsive row (Watch Spaces, Anime Tracked, Episodes Together).
3. **Watch Spaces grid** — `bloom-grid-auto-md` card grid showing each watch space with avatars, quick stats, and a link.
4. **Recent Activity** — Up to 3 horizontal cards showing recently updated anime with cover, title, watch space name, and status badge.
5. **Quick Actions** — "Create Watch Space" (primary) and "Browse Anime" (secondary) buttons.
6. **Empty state** — When no watch spaces exist, replace sections 2-4 with a friendly onboarding card and CTA.

**Layout follows existing patterns**: Uses `bloom-card`, `bloom-badge`, `bloom-avatar-stack`, `bloom-button` from the shared UI library. No new shared components needed.

### 6. Loading and error states

**Decision**: Use skeleton cards matching the layout while `GET /home/overview` is in-flight. Show an error card with retry button on failure. Follow the patterns established in the UI/UX doctrine.

## Risks / Trade-offs

- **Cross-module query coupling**: The Home endpoint reads from Identity, WatchSpaces, and AnimeTracking. If one module's query interface changes, the Home endpoint must be updated. → Mitigation: Keep the orchestrator thin and map to DTOs immediately. Use integration tests that exercise the full endpoint.

- **Performance of aggregated query**: Summing stats across all watch spaces could be slow for users with many spaces. → Mitigation: The query is bounded by the user's membership count (expected: 1-5 watch spaces). For v1 this is fine. If needed later, introduce a materialised summary table.

- **N+1 risk for recent activity**: Fetching the 3 most recently updated anime across all watch spaces naïvely could mean querying each space individually. → Mitigation: The AnimeTracking module should expose a single query that accepts a list of watch space IDs and returns the top N most recently updated anime globally, ordered by `lastUpdatedAt` descending.

- **Feature directory rename**: Renaming `dashboard/` to `home/` touches the route config and import paths. → Mitigation: Small scope — only one route config line and the feature directory. No external consumers.
