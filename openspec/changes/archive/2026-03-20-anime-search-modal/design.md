## Context

BloomWatch's watch space detail page currently has no way to add anime. The backend already exposes `GET /api/anilist/search?query=<term>` (AniList GraphQL proxy with caching) and `POST /watchspaces/{id}/anime` for adding anime to a space. The frontend needs a search-and-select UI.

The UI component library (`shared/ui/`) provides `bloom-button`, `bloom-input`, `bloom-card`, `bloom-badge`, and `bloom-avatar` — all standalone Angular 21 components using signals. There is **no existing modal/dialog component**; current flows use inline forms or `confirm()`. A reusable modal is needed.

## Goals / Non-Goals

**Goals:**
- Provide a smooth, keyboard-accessible modal for searching anime by title
- Display rich search results (cover art, title, format, episodes, genres)
- Allow one-click add of a selected anime to the current watch space
- Deliver a reusable `bloom-modal` component that other features can adopt

**Non-Goals:**
- Advanced filtering (by genre, year, format) — search by title is sufficient for now
- Pagination / infinite scroll of results — AniList returns a reasonable page of results
- Editing anime details post-add from within this modal
- Server-side search suggestions or autocomplete dropdown — just a debounced text search

## Decisions

### 1. Reusable `bloom-modal` as a shared UI component

**Decision:** Create `bloom-modal` in `shared/ui/modal/` following existing component conventions (standalone, signals API, BEM SCSS, content projection).

**Rationale:** The codebase currently lacks any overlay/dialog pattern. A modal will be needed again (anime detail view, confirmations, settings). Building it as a shared component avoids duplication.

**Alternatives considered:**
- CDK Dialog / Angular CDK overlay — adds a dependency the project doesn't use; the modal needs are simple enough to implement directly
- Inline expandable panel — doesn't provide the focused interaction needed for search-and-select

### 2. Modal renders in-place with a fixed-position overlay

**Decision:** The modal uses `position: fixed` with a backdrop overlay, rendered where it's placed in the template (no portal). The component manages body scroll lock.

**Rationale:** Angular's CDK portals add complexity. Since the modal is always placed at a top-level position in the host component's template, z-index stacking is straightforward.

### 3. Debounced search with signals

**Decision:** Use a `signal` for the search query, an `effect` to debounce and trigger the HTTP call, and a `signal` for results/loading/error state. No RxJS-heavy patterns.

**Rationale:** The codebase uses signals throughout (no NgRx/RxJS state management). A 300ms debounce via `setTimeout` in an `effect` keeps the pattern consistent. The `ApiService` returns `Observable`, so we subscribe once per search and store the result in a signal.

### 4. Anime search modal as a feature component

**Decision:** Create `anime-search-modal` in `features/watch-spaces/` as a standalone component that composes `bloom-modal`, `bloom-input`, `bloom-button`, and `bloom-badge`.

**Rationale:** The search modal is tightly coupled to the watch space context (it needs the `watchSpaceId` to add anime). It's not generic enough for `shared/ui/`.

### 5. Service methods on `WatchSpaceService`

**Decision:** Add `searchAnime(query: string)` and `addAnimeToWatchSpace(spaceId, body)` methods to the existing `WatchSpaceService`.

**Rationale:** These methods serve the watch-space feature flow. Keeping them co-located with other watch-space API calls maintains the single-service-per-feature pattern. The search endpoint lives under `/api/anilist/search` but is consumed in the watch-space context.

## Risks / Trade-offs

- **[No focus-trap library]** → Implement a lightweight focus trap (query focusable elements, wrap Tab key). Sufficient for a single modal; if multiple stacked modals are needed later, revisit with a dedicated library.
- **[Body scroll lock on mobile]** → Use `overflow: hidden` on `document.body` when modal opens. Known limitation: iOS Safari may need `position: fixed` on body — handle if reported.
- **[Search rate limiting]** → AniList has rate limits; the backend already caches results. The 300ms debounce further reduces request volume. No additional mitigation needed.
