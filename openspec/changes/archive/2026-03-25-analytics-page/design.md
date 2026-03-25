## Context

The dashboard (Story 10.1) provides a high-level summary by calling a single composite `GET /watchspaces/{id}/dashboard` endpoint. Story 10.3 adds a dedicated analytics page that calls three separate analytics endpoints in parallel for more detailed data: full rating gap list (not top-3), compatibility breakdown stats, and comprehensive shared history stats. A wireframe exists at `docs/wireframes/analytics.html`.

Backend endpoints already exist and return:
- `GET /watchspaces/{id}/analytics/compatibility` → `CompatibilityScoreResult` (score, averageGap, ratedTogetherCount, label, or null with message)
- `GET /watchspaces/{id}/analytics/rating-gaps` → `RatingGapsResult` (items sorted by descending gap, each with member ratings)
- `GET /watchspaces/{id}/analytics/shared-stats` → `SharedStatsResult` (totalEpisodesWatchedTogether, totalFinished, totalDropped, totalWatchSessions, mostRecentSessionDate)

## Goals / Non-Goals

**Goals:**
- Dedicated analytics page at `/watch-spaces/:id/analytics` with full compatibility, stats, and rating gap details
- Reuse `bloom-compat-ring` component for the compatibility ring visualization
- Grouped bar chart comparing member ratings using `ng2-charts` (Chart.js wrapper)
- Parallel API loading with independent section states (each section can show data, loading, or empty independently)
- Responsive two-column → single-column layout

**Non-Goals:**
- Filtering or sorting controls on the rating gaps list (show all, sorted by gap desc as returned by API)
- Caching or local storage of analytics data
- Export/share analytics data
- Pagination of rating gaps (API returns all items)

## Decisions

### 1. Component file location: `features/watch-spaces/watch-space-analytics.ts`
**Rationale:** Follows the existing pattern — `watch-space-dashboard.ts`, `watch-space-detail.ts`, `anime-detail.ts` are all flat files in the `watch-spaces` feature folder. No subfolder needed for a single-component page.

### 2. Chart library: `ng2-charts` with `chart.js`
**Rationale:** The wireframe specifies a grouped bar chart. `ng2-charts` is the standard Angular wrapper for Chart.js — lightweight, well-maintained, and provides `BaseChartDirective` for declarative chart binding. Alternative `ngx-charts` is heavier and SVG-based, unnecessary for a single bar chart.
**Installation:** `npm install ng2-charts chart.js`

### 3. Parallel API calls with `forkJoin`
**Rationale:** The three endpoints are independent. Using `forkJoin` fires all three in parallel and resolves when all complete. Individual error handling per section via `catchError` so one failing endpoint doesn't block the others. Each section gets its own `signal()` for data, allowing partial rendering.

**Alternative considered:** Sequential calls — rejected because it adds unnecessary latency (3x serial vs 1x parallel).

### 4. Rating gap bars: inline CSS width percentage
**Rationale:** The wireframe shows horizontal bars per rater scaled to their score (score/10 * 100%). This is a simple `[style.width.%]` binding — no charting library needed for these bars. The Chart.js grouped bar chart is a separate section showing all ratings side-by-side.

### 5. Route: flat sibling at `:id/analytics`
**Rationale:** Consistent with existing flat route structure (`:id`, `:id/manage`, `:id/anime/:animeId`). No nesting or layout wrapper needed.

## Risks / Trade-offs

- **[Large rating gaps list]** → If a watch space has many rated anime, the full list renders all at once. Acceptable for MVP; virtual scrolling could be added later if needed.
- **[Chart.js bundle size]** → Adds ~60KB gzipped. Mitigated by the analytics route being lazy-loaded, so the chart code only loads when the user navigates to analytics.
- **[Three parallel requests]** → If one fails, that section shows its own error state while others render normally. This is better UX than all-or-nothing.
