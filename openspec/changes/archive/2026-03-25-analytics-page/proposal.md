## Why

The dashboard provides a high-level overview with top-3 rating gaps and a single compatibility ring. Members need a dedicated analytics page to explore their full taste alignment in detail — all rating gaps with visual comparison bars, a grouped bar chart, compatibility breakdown stats, and comprehensive shared history stats.

## What Changes

- Add a new route `/watch-spaces/:id/analytics` with a dedicated analytics page component
- Add TypeScript interfaces for the three analytics API responses (`CompatibilityScoreResult`, `RatingGapsResult`, `SharedStatsResult`)
- Add service methods calling `GET /analytics/compatibility`, `GET /analytics/rating-gaps`, and `GET /analytics/shared-stats` in parallel
- Render a two-column layout: compatibility ring with breakdown stats (left), shared stats grid with recent session date (right)
- Render the full rating gaps list with per-member score bars, scores, and gap delta
- Render a grouped bar chart (using `ng2-charts` / Chart.js) comparing member ratings side-by-side
- Add a "View Analytics" link from the dashboard to the analytics page
- Handle loading skeletons, error/retry, and empty/insufficient-data states per section
- Responsive: collapses to single column on mobile

## Capabilities

### New Capabilities
- `analytics-page`: Dedicated analytics page with compatibility breakdown, shared stats, full rating gaps list, and rating comparison chart

### Modified Capabilities
- `watch-space-dashboard`: Add navigation link from dashboard to the analytics page

## Impact

- **New files:** analytics page component (`.ts`, `.html`, `.scss`, `.spec.ts`), new TypeScript interfaces in `watch-space.model.ts`, new service methods in `watch-space.service.ts`
- **Modified files:** `watch-spaces.routes.ts` (new route), `watch-space-dashboard.html` (analytics link)
- **New dependency:** `ng2-charts` and `chart.js` npm packages for the grouped bar chart
- **APIs consumed:** `GET /watchspaces/{id}/analytics/compatibility`, `GET /watchspaces/{id}/analytics/rating-gaps`, `GET /watchspaces/{id}/analytics/shared-stats`
