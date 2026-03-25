## Context

The backend `GET /watchspaces/{id}/dashboard` endpoint (Story 5.1) is fully implemented, returning a composite `DashboardSummaryResult` with stats, compatibility, currently-watching (up to 5), backlog highlights (up to 5), and rating-gap highlights (up to 3). No frontend code exists to consume it.

Currently, `/watch-spaces/:id` routes to `WatchSpaceDetail`, which renders the anime list, member management, invitations, and settings in a single page. The dashboard needs to become the default view for a watch space, with the existing functionality accessible via sub-navigation.

The wireframe (`docs/wireframes/dashboard.html`) defines the layout: stat cards row, compatibility ring + random pick (two-column), currently watching grid, backlog highlights grid, and rating gap list.

## Goals / Non-Goals

**Goals:**
- Build the dashboard page component consuming `GET /watchspaces/{id}/dashboard`
- Route `/watch-spaces/:id` to the dashboard as the default view
- Render all 5 dashboard sections with proper loading, error, and empty states
- Provide navigation between dashboard and the existing anime list / settings views
- Match the wireframe layout and kawaii/Y2K design system

**Non-Goals:**
- Compatibility score display component (Story 10.2 — will be a standalone reusable component later; for now, render inline)
- Random backlog picker component with reroll (Story 10.4 — not part of this story)
- Full analytics page (Story 10.3)
- Modifying the backend endpoint

## Decisions

### 1. Route restructuring — child routes under `:id`

**Decision:** Use child routes under `/watch-spaces/:id` with a layout wrapper component.

```
/watch-spaces/:id           → redirects to /watch-spaces/:id/dashboard
/watch-spaces/:id/dashboard → WatchSpaceDashboard (new)
/watch-spaces/:id/manage    → WatchSpaceDetail (existing, renamed route)
/watch-spaces/:id/anime/:animeId → AnimeDetail (unchanged)
```

**Rationale:** A parent layout component (e.g., `WatchSpaceLayout`) reads the `:id` param once and provides sub-navigation (Dashboard | Anime List | Settings). This avoids duplicating header/nav across pages. The existing `WatchSpaceDetail` component and template remain unchanged — only its route path changes.

**Alternatives considered:**
- Embedding the dashboard in the existing `WatchSpaceDetail` component — rejected because it would make the already large component even larger
- Two separate top-level routes without a layout — rejected because it duplicates the space header and back-link

### 2. Dashboard component structure — single component with inline sections

**Decision:** Build one `WatchSpaceDashboard` component with all 5 sections rendered inline (stat cards, compatibility, currently watching, backlog, rating gaps). No child components for now.

**Rationale:** Story 10.2 will extract compatibility into a reusable component later. For now, keeping everything in one component is simpler and avoids premature abstraction. Each section is small enough (10-30 lines of template) to stay manageable.

### 3. TypeScript models — extend `watch-space.model.ts`

**Decision:** Add dashboard response interfaces to the existing `watch-space.model.ts` file, matching the backend `DashboardSummaryResult` shape with camelCase field names.

Key types:
- `DashboardSummary` (top-level response)
- `DashboardStats`
- `DashboardCompatibility`
- `DashboardCurrentlyWatchingItem`
- `DashboardBacklogHighlight`
- `DashboardRatingGapHighlight`
- `DashboardRater`

### 4. Compatibility ring — SVG with CSS custom properties

**Decision:** Render the compatibility score as an SVG circular ring matching the wireframe, using `stroke-dashoffset` computed from the score. Color varies by score range (green 80+, yellow 50–79, pink <50). When `compatibility` is null, show a placeholder message.

**Rationale:** SVG ring is lightweight, accessible, and matches the wireframe exactly. No third-party charting library needed.

### 5. Loading state — skeleton placeholders

**Decision:** Show placeholder skeleton blocks (pulsing rectangles) for each dashboard section while loading, rather than a single spinner. This gives users a sense of the page structure before data arrives.

### 6. Service method — single `getDashboard()` call

**Decision:** Add `getDashboard(spaceId: string): Observable<DashboardSummary>` to `WatchSpaceService`. One API call populates the entire page.

## Risks / Trade-offs

**[Route change breaks existing bookmarks/links]** → The existing `/watch-spaces/:id` URL currently shows the anime list. Changing it to the dashboard may confuse users with bookmarks. → *Mitigation:* The redirect from `:id` to `:id/dashboard` is transparent. Links to the anime list (e.g., from other pages) should be updated to `:id/manage` during this change.

**[Large single component]** → The dashboard component template will be ~150-200 lines of HTML. → *Mitigation:* Each section is clearly demarcated with comments and BEM class names. Story 10.2 will extract the compatibility ring. This is acceptable for now.

**[No random pick in this story]** → The wireframe shows a "Pick Something to Watch" card, but Story 10.4 covers the random picker separately. → *Mitigation:* Omit the random pick card from this story's dashboard. It will be added by Story 10.4 later.
