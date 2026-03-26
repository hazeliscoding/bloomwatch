## Context

The dashboard wireframe (`docs/wireframes/dashboard.html`) shows the compatibility ring and a "Pick Something to Watch" card side-by-side in a two-column layout. The current dashboard has the compat section as a full-width card. This change adds the picker alongside it.

The backend endpoint `GET /watchspaces/{id}/analytics/random-pick` returns a `RandomPickResult` with a nullable `Pick` (containing title, cover, episodes, mood, vibe, pitch) and a `Message` when the backlog is empty.

## Goals / Non-Goals

**Goals:**
- Standalone `bloom-backlog-picker` component reusable anywhere a `spaceId` is available
- Fetches a random pick on init and on reroll button click
- Displays cover image, title, episode count, mood/vibe/pitch badges
- Click-to-navigate to anime detail page
- Empty backlog state with descriptive message
- Loading spinner/skeleton during API calls
- Embed on dashboard in two-column layout with compatibility

**Non-Goals:**
- Filtering by genre, mood, or other criteria before picking
- Persisting or "locking in" a pick
- Placing the component on the anime list page (the story says "optionally" — defer to a follow-up)

## Decisions

### 1. Component location: `shared/ui/backlog-picker/bloom-backlog-picker.ts`
**Rationale:** Follows the same pattern as `bloom-compat-ring` — a shared UI component that can be embedded in any page. Takes `spaceId` as input and manages its own API calls internally.

### 2. Component owns its own data fetching
**Rationale:** Unlike the compat ring (which receives data from the parent), the picker has its own "reroll" interaction that triggers new API calls. Having the component own its fetch cycle (via injected `WatchSpaceService`) keeps the parent template simple: just `<bloom-backlog-picker [spaceId]="spaceId" />`.

### 3. Dashboard layout: wrap compat + picker in existing `dashboard__two-col` grid
**Rationale:** The dashboard already has a `.dashboard__two-col` class for two-column grids. Wrap the compatibility card and the picker card in a `dashboard__two-col` section. On mobile, this collapses to single column via the existing `bp-mobile-only` breakpoint.

### 4. Navigation via output event
**Rationale:** The picker emits a `(picked)` event with the `watchSpaceAnimeId` when the user clicks "View Details". The parent dashboard handles navigation. This keeps the component decoupled from routing.

## Risks / Trade-offs

- **[Multiple rapid rerolls]** → Each reroll fires a new HTTP request. Acceptable for MVP; no debouncing needed since the endpoint is lightweight.
- **[No cover image]** → If `coverImageUrlSnapshot` is null, show the standard kawaii gradient placeholder. Same pattern as other components.
