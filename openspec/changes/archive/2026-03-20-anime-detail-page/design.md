## Context

The anime detail page is the deepest navigation level in the watch space feature: `WatchSpaceList → WatchSpaceDetail → AnimeDetail`. The route (`/watch-spaces/:id/anime/:animeId`) is already wired and navigation from the anime list grid works, but the `AnimeDetail` component is a stub returning `<h1>Anime Detail</h1>`.

All backend endpoints needed are already implemented and tested:
- `GET /watchspaces/{id}/anime/{animeId}` — full detail with participants + sessions
- `PATCH .../participant-progress` — update own status + episodes
- `PATCH .../participant-rating` — submit/update rating (0.5–10.0)
- `POST .../sessions` — record a watch session
- `PATCH ...` — update shared anime status

The frontend service (`WatchSpaceService`) currently has no methods for these detail/mutation endpoints. The model file has no interfaces for the detail response DTO.

## Goals / Non-Goals

**Goals:**
- Implement a fully functional anime detail page consuming all existing backend endpoints
- Follow established Angular patterns (standalone components, signals, BEM SCSS, Bloom component library)
- Provide a clear visual hierarchy: metadata hero → shared state → actions → participants → sessions
- Support all mutation flows: update progress, submit rating, record session, update shared status
- Match the kawaii/Y2K design system with appropriate Bloom components and tokens

**Non-Goals:**
- Real-time updates / WebSocket sync between participants (polling or manual refresh is fine)
- Optimistic UI updates — wait for server confirmation before reflecting changes
- AniList deep-link or external metadata enrichment beyond what's already snapshot
- Inline editing of mood/vibe/pitch (this can be a follow-up; focus is on read + core mutations)
- Responsive mobile-first layout polish (functional on mobile, but pixel-perfect responsive refinement is follow-up)

## Decisions

### 1. Single component with collapsible sections vs. routed sub-views

**Decision:** Single `AnimeDetail` component with logical template sections, not child routes.

**Why:** The page is a single API call (`GetWatchSpaceAnimeDetail`) that returns all data. There's no independent loading for sub-sections. Child routes would add routing complexity without benefit. The anime list component uses the same single-component pattern successfully.

**Alternative considered:** Tab-based sub-views (Info / Progress / Sessions). Rejected because the data volume per section is small — tabs would add clicks without reducing cognitive load.

### 2. Service methods and interfaces added to existing files

**Decision:** Add new methods to `WatchSpaceService` and new interfaces to `watch-space.model.ts` rather than creating separate files.

**Why:** The existing service and model files are cohesive around the watch-space feature domain and are not oversized. This follows the established pattern where all watch-space API interactions live in one service.

### 3. Signal-based state management

**Decision:** Use Angular signals (`signal()`, `computed()`) for all component state, consistent with `AnimeListComponent`.

**Why:** The codebase has already standardized on signals over BehaviorSubject for component state. Keeps the pattern uniform.

### 4. Form interaction pattern for mutations

**Decision:** Use inline form sections (not modals) for progress updates, rating, and session recording. Each action section has a toggle to expand/collapse the form.

**Why:** Modals interrupt flow and the anime search modal already uses that pattern for a different purpose (search + add). Inline forms keep the user on the detail page with context visible. The forms are small (2–4 fields each) and don't warrant modal overhead.

**Alternative considered:** A single "edit mode" that makes all fields editable at once. Rejected because the three mutations hit different endpoints and have different validation — treating them as separate actions is clearer.

### 5. Back navigation

**Decision:** Include a back link/button that navigates to the watch space detail page (`/watch-spaces/:id`), preserving the breadcrumb mental model.

**Why:** The browser back button works but an explicit back affordance is standard in detail pages and helps orientation.

### 6. Rating display — star-based vs. numeric

**Decision:** Display ratings as numeric scores (e.g., "8.5 / 10") with a visual bar, not star icons.

**Why:** The backend supports 0.5–10.0 in 0.5 increments (20 possible values). Stars typically map to 5 or 10 discrete values and would either lose precision or require half-star rendering. A numeric display with a colored progress bar is simpler and aligns with the design system's use of badges and progress indicators.

## Risks / Trade-offs

**[Risk] Stale data after mutations** → After each successful mutation (progress update, rating, session), re-fetch the full detail from the API to ensure consistency. This is slightly more network traffic than patching local state, but guarantees correctness and avoids divergence bugs.

**[Risk] Large participant/session lists** → For watch spaces with many members or long-running series with dozens of sessions, the page could get long. → Mitigate with collapsible sections and a "show more" pattern for sessions beyond 10. This is unlikely to be a real problem in practice (watch spaces are small groups).

**[Risk] Cover image loading** → Cover images are AniList URLs snapshotted at add-time. If AniList is slow or the image is gone, the hero section needs a graceful fallback. → Use a placeholder gradient (matching the anime list pattern) when the image fails to load.
