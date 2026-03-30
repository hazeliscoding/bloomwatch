## 1. Backend — Home Overview Endpoint

- [x] 1.1 Create `HomeEndpoints.cs` in `BloomWatch.Api/Modules/Home/` with `GET /home/overview` mapped as an authenticated endpoint
- [x] 1.2 Create `GetHomeOverviewQuery` and `HomeOverviewResult` DTOs (display name, stats, watch space summaries, recent activity)
- [x] 1.3 Implement `GetHomeOverviewQueryHandler` as a thin orchestrator that calls existing Identity, WatchSpaces, and AnimeTracking query handlers to gather data
- [x] 1.4 Implement stats aggregation logic: count watch spaces, deduplicate anime by AniList ID for total anime tracked, sum shared episodes watched
- [x] 1.5 Implement watch space summaries with per-space `watchingCount` and `backlogCount` by querying AnimeTracking for each space's anime status counts
- [x] 1.6 Implement recent activity query: fetch top 3 most recently updated anime across all user's watch spaces ordered by `lastUpdatedAt` descending
- [x] 1.7 Register `MapHomeEndpoints()` in `Program.cs`
- [x] 1.8 Write integration tests for `GET /home/overview` covering: authenticated success, empty state (no watch spaces), unauthenticated returns 401

## 2. Frontend — Rename Dashboard to Home

- [x] 2.1 Rename `src/app/features/dashboard/` directory to `src/app/features/home/`
- [x] 2.2 Update `app.routes.ts` to import from `./features/home/home.routes` instead of `./features/dashboard/dashboard.routes`
- [x] 2.3 Update `shell-layout.html` nav link text from "Dashboard" to "Home" and update the icon

## 3. Frontend — Home Page API Service

- [x] 3.1 Create `HomeService` in `src/app/features/home/` with a method to call `GET /home/overview` and return typed response
- [x] 3.2 Define TypeScript interfaces for `HomeOverviewResponse`, `HomeStats`, `HomeWatchSpaceSummary`, and `HomeRecentActivity`

## 4. Frontend — Home Page Component

- [x] 4.1 Create the `Home` component as a standalone component in `src/app/features/home/home.ts` with signal-based state management
- [x] 4.2 Implement the greeting section with "Welcome back, {displayName}" using display font and gradient text
- [x] 4.3 Implement the global stats strip: 3 stat cards (Watch Spaces, Anime Tracked, Episodes Together) in a responsive row
- [x] 4.4 Implement the watch space card grid using `bloom-grid-auto-md` with `bloom-card`, `bloom-avatar-stack`, `bloom-badge`, and navigation link to `/watch-spaces/{id}`
- [x] 4.5 Implement the recent activity section showing up to 3 items with cover image, title, watch space name, status badge, and relative timestamp
- [x] 4.6 Implement quick-action buttons: primary "Create Watch Space" and secondary "Browse Anime"
- [x] 4.7 Implement the empty state onboarding card when the user has no watch spaces
- [x] 4.8 Implement loading skeleton state matching the layout of all sections
- [x] 4.9 Implement error state with "Something went wrong" message and retry button

## 5. Frontend — Styling

- [x] 5.1 Create `home.scss` with BEM-structured styles using only `--bloom-*` design tokens, following the UI/UX doctrine
- [x] 5.2 Ensure responsive layout: stat cards stack on mobile, grid collapses gracefully, spacing scales per breakpoints
- [x] 5.3 Add `prefers-reduced-motion` overrides for any animations on the home page

## 6. Verification

- [x] 6.1 Verify the Home page renders correctly with data, empty state, loading state, and error state
- [x] 6.2 Verify shell nav shows "Home" label and active state at `/`
- [x] 6.3 Verify all links navigate correctly (watch space detail, anime detail, create watch space, browse anime)
- [x] 6.4 Verify WCAG AA compliance: keyboard navigation, screen reader labels, colour contrast, focus indicators
