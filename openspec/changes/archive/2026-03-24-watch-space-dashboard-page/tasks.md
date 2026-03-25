## 1. Models and Service

- [x] 1.1 Add dashboard TypeScript interfaces to `watch-space.model.ts` (DashboardSummary, DashboardStats, DashboardCompatibility, DashboardCurrentlyWatchingItem, DashboardBacklogHighlight, DashboardRatingGapHighlight, DashboardRater)
- [x] 1.2 Add `getDashboard(spaceId: string)` method to `WatchSpaceService`
- [x] 1.3 Write unit tests for `getDashboard` in `watch-space.service.spec.ts`

## 2. Route Restructuring

- [x] 2.1 Update `watch-spaces.routes.ts` to add child routes under `:id` — redirect `:id` to `:id/dashboard`, add `:id/dashboard` for new dashboard component, move existing `WatchSpaceDetail` to `:id/manage`
- [x] 2.2 Add `<router-outlet>` or layout wrapper if needed for the child route structure
- [x] 2.3 Update any internal navigation links that point to `/watch-spaces/:id` to use the new route paths (e.g., from watch-space-list, invitation-response)

## 3. Dashboard Component — Scaffold and Loading/Error States

- [x] 3.1 Create `watch-space-dashboard.ts` component with signals for dashboard data, loading state, and error state; fetch dashboard data on init
- [x] 3.2 Create `watch-space-dashboard.html` template with loading skeleton placeholders for all 5 sections
- [x] 3.3 Add error state with retry button in the template
- [x] 3.4 Create `watch-space-dashboard.scss` with BEM styles for the dashboard layout, stat cards grid, and loading skeletons

## 4. Dashboard Component — Stat Cards Section

- [x] 4.1 Add stat cards row template: 4-column grid with Total Shows, Currently Watching, Finished, Episodes Together
- [x] 4.2 Add stat card SCSS styles (number + label layout, color-coded numbers)

## 5. Dashboard Component — Compatibility Section

- [x] 5.1 Add compatibility section template with SVG circular ring, score text, label, and rated-together context
- [x] 5.2 Add computed properties for ring stroke-dashoffset and ring color based on score range (green 80+, yellow 50–79, pink <50)
- [x] 5.3 Add null-compatibility placeholder message ("Not enough ratings yet")
- [x] 5.4 Add compatibility section SCSS styles (ring sizing, colors, label layout)

## 6. Dashboard Component — Currently Watching Grid

- [x] 6.1 Add currently-watching grid template: cover image (or placeholder), title, progress bar, episode label, click-to-navigate
- [x] 6.2 Add computed helpers for progress bar width percent and episode label formatting
- [x] 6.3 Add empty state for when no anime are currently being watched
- [x] 6.4 Add currently-watching grid SCSS styles (responsive grid, card layout, progress bar)

## 7. Dashboard Component — Backlog Highlights Grid

- [x] 7.1 Add backlog highlights grid template: cover image (or placeholder), title, Backlog badge, click-to-navigate
- [x] 7.2 Add empty state for when backlog is empty
- [x] 7.3 Add backlog highlights SCSS styles

## 8. Dashboard Component — Rating Gap Highlights

- [x] 8.1 Add rating gap highlights list template: small cover thumbnail, anime title, per-rater name + score, delta value, click-to-navigate
- [x] 8.2 Add empty state for when no rating gaps exist
- [x] 8.3 Add rating gap SCSS styles (gap card layout, score display, delta badge)

## 9. Dashboard Component — Header and Navigation

- [x] 9.1 Add dashboard header template: watch space name, back link to watch spaces list, navigation buttons (Anime List, Settings, Add Anime)
- [x] 9.2 Add header SCSS styles
- [x] 9.3 Wire navigation links: "Anime List" → `:id/manage`, "Add Anime" → opens search modal or navigates to manage view

## 10. Tests

- [x] 10.1 Write dashboard component unit tests: loading state renders skeletons, successful load renders all sections, error state renders with retry, retry re-fetches data
- [x] 10.2 Write dashboard component unit tests: stat cards display correct values, stat cards display zeroes for empty space
- [x] 10.3 Write dashboard component unit tests: compatibility ring renders with score/label/context, null compatibility shows placeholder
- [x] 10.4 Write dashboard component unit tests: currently-watching cards render with progress bars, empty state renders correctly, card click navigates
- [x] 10.5 Write dashboard component unit tests: backlog cards render, rating gap entries render with scores and delta, empty states render
- [x] 10.6 Write route integration tests: `/watch-spaces/:id` redirects to dashboard, `/watch-spaces/:id/manage` renders WatchSpaceDetail
