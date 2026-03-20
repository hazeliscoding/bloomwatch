## Context

The watch space detail page (`/watch-spaces/:id`) currently shows space metadata, member management, invitations, and an "Add Anime" button wired to the search modal (Story 9.1). The backend `GET /watchspaces/{id}/anime?status=` endpoint is fully implemented and returns anime items with participant progress arrays. The frontend `WatchSpaceAnimeListItem` model lacks the `participants` field, and there is no UI to display the list.

The Angular app uses standalone components with signals-based state management, the `bloom-*` shared component library (cards, badges, buttons, inputs, modals), and SCSS with BEM conventions following the kawaii/Y2K design system.

## Goals / Non-Goals

**Goals:**
- Render a filterable anime list on the watch space detail page, grouped by shared status
- Show per-anime participant progress so members can see where everyone is at a glance
- Enable navigation from the list to the anime detail page
- Refresh the list automatically after adding anime via the search modal

**Non-Goals:**
- Inline editing of episode progress or shared status (Story 9.4 scope)
- Pagination or infinite scroll (list sizes are small per watch space)
- Anime detail page itself (Story 9.3 scope)
- Search/sort within a status tab

## Decisions

### 1. Standalone `AnimeListComponent` composed into the detail page

Render the anime list as a standalone Angular component (`AnimeListComponent`) that receives the `watchSpaceId` as an input and manages its own data fetching and tab state. This keeps the already-large `WatchSpaceDetail` component focused on space management.

**Alternative considered:** Inline everything in `WatchSpaceDetail` — rejected because it would further bloat an already complex component (~280 lines).

### 2. Status tabs as a signal-driven filter, single API call approach

Use client-side filtering with a single `listWatchSpaceAnime()` call (no status param) that fetches all anime, then filter locally by the selected tab. This avoids a network round-trip on every tab switch and the dataset is small (typically < 50 anime per space).

**Alternative considered:** Server-side filtering per tab — rejected because the data volume is low and UX is snappier with local filtering. The `?status=` query param remains available for future use if lists grow large.

### 3. Inline anime cards, not a separate `AnimeCardComponent`

Use inline card markup within `AnimeListComponent`'s template, wrapping each item in `<bloom-card>`. The card structure is specific to this list and unlikely to be reused elsewhere without the detail page card having different layout needs.

**Alternative considered:** Separate `AnimeCardComponent` — premature abstraction for a single use site. Can extract later if Story 9.4 or other views need it.

### 4. Extend `WatchSpaceAnimeListItem` with participants array

Add a `participants` field to the existing `WatchSpaceAnimeListItem` interface to match the API response. Each participant entry includes `userId`, `displayName`, `individualStatus`, and `episodesWatched`.

### 5. Refresh list on search modal close

Wire the existing `onSearchModalClosedWithRefresh()` callback to trigger a reload of the anime list. The `AnimeListComponent` exposes a `refresh()` method or reacts to a signal/input change.

## Risks / Trade-offs

- **Full list fetch on every refresh** — Acceptable for small lists but could degrade if a space has hundreds of anime. → Mitigation: Can add server-side filtering later without UI changes since tabs map directly to status values.
- **No optimistic UI for status/progress updates** — Out of scope (Story 9.4), but the list component should be structured to allow signal-based updates later. → Mitigation: Use signals for list data so downstream changes are reactive.
