## 1. Models and Service Layer

- [x] 1.1 Add TypeScript interfaces to `watch-space.model.ts`: `CompatibilityScoreResult`, `RatingGapsResult`, `RatingGapItem`, `SharedStatsResult` (reuse existing `DashboardCompatibility` and `DashboardRater` where shapes match)
- [x] 1.2 Add service methods to `watch-space.service.ts`: `getCompatibility(spaceId)`, `getRatingGaps(spaceId)`, `getSharedStats(spaceId)` calling the three analytics endpoints
- [x] 1.3 Write unit tests in `watch-space.service.spec.ts` for the three new service methods

## 2. Dependencies and Routing

- [x] 2.1 Install `ng2-charts` and `chart.js` npm packages
- [x] 2.2 Add route `{ path: ':id/analytics', component: WatchSpaceAnalytics }` to `watch-spaces.routes.ts`

## 3. Analytics Page Component

- [x] 3.1 Create `watch-space-analytics.ts` with three parallel API calls via `forkJoin` (with per-section `catchError`), signals for each section's data/loading/error state, and chart data computed from rating gaps
- [x] 3.2 Create `watch-space-analytics.html` with header (back link + title), two-column layout (compatibility card with ring + breakdown, shared stats card with 2x2 grid + recent session), rating gaps list with score bars, grouped bar chart section, and skeleton/error/empty states per section
- [x] 3.3 Create `watch-space-analytics.scss` with BEM styles for all sections: two-column layout, stats grid, breakdown panel, gap rows with score bars, chart container, legend, skeletons, responsive breakpoint, and reduced-motion support

## 4. Dashboard Navigation Link

- [x] 4.1 Add "Analytics" button to `watch-space-dashboard.html` header actions and `navigateToAnalytics()` method in `watch-space-dashboard.ts`

## 5. Tests

- [x] 5.1 Write unit tests in `watch-space-analytics.spec.ts`: page renders all sections with data, empty/null states per section, error state with retry, back link and header, chart renders with rating data
