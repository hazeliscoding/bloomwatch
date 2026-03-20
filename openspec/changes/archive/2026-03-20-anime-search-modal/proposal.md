## Why

Users currently have no way to search for and add anime to their watch spaces from the UI. The backend AniList search endpoint (`GET /api/anilist/search`) exists but has no corresponding frontend interface. Without this, users cannot populate their watch spaces — the core action that drives all downstream features (tracking, ratings, compatibility).

## What Changes

- Add a reusable **bloom-modal** shared UI component (overlay dialog with backdrop, close button, keyboard/focus-trap support)
- Add an **anime-search-modal** feature component that:
  - Opens as a modal overlay from the watch space detail view
  - Provides a debounced text input to search anime via the existing backend endpoint
  - Displays search results as a scrollable list with cover image, title, format, episodes, season/year, and genres
  - Allows selecting an anime to add it to the current watch space via `POST /watchspaces/{id}/anime`
  - Shows loading, empty, and error states
- Wire the modal into the watch space detail page with an "Add Anime" trigger button

## Capabilities

### New Capabilities
- `anime-search-modal`: Modal dialog for searching AniList anime and adding a selection to a watch space. Covers the search input, result display, selection action, and integration with the watch space detail view.
- `bloom-modal`: Reusable modal/dialog shared UI component with backdrop overlay, close-on-escape, focus trapping, and content projection.

### Modified Capabilities
<!-- No existing spec-level requirements are changing. The modal consumes existing endpoints as-is. -->

## Impact

- **Frontend (BloomWatch.UI)**:
  - New shared component: `bloom-modal` in `shared/ui/modal/`
  - New feature component: `anime-search-modal` in `features/watch-spaces/`
  - Modified: `watch-space-detail` component (adds "Add Anime" button and modal trigger)
  - Modified: `watch-space.service.ts` (add `searchAnime()` and `addAnimeToWatchSpace()` methods if not already present)
  - Modified: `watch-space.model.ts` (add `AnimeSearchResult` and `AddAnimeRequest` interfaces)
  - Modified: `shared/ui/index.ts` (export new bloom-modal component)
- **Backend**: No changes — uses existing `GET /api/anilist/search` and `POST /watchspaces/{id}/anime` endpoints
- **Dependencies**: None new
