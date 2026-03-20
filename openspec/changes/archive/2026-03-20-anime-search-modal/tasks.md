## 1. Shared Modal Component

- [x] 1.1 Create `bloom-modal` component scaffold (`shared/ui/modal/bloom-modal.ts` and `bloom-modal.scss`) with `open` input signal, `closed` output, and content projection slots (header, body, footer)
- [x] 1.2 Implement fixed-position overlay with backdrop, close-on-backdrop-click, and close-on-Escape
- [x] 1.3 Implement focus trap (Tab/Shift+Tab wrapping) and auto-focus first focusable element on open
- [x] 1.4 Implement body scroll lock on open, restore on close
- [x] 1.5 Add configurable `width` input (default `32rem`)
- [x] 1.6 Export `BloomModalComponent` from `shared/ui/index.ts`
- [x] 1.7 Write unit tests for bloom-modal (open/close, Escape key, backdrop click, focus trap)

## 2. Data Layer

- [x] 2.1 Add `AnimeSearchResult` and `AddAnimeToWatchSpaceRequest` interfaces to `watch-space.model.ts`
- [x] 2.2 Add `searchAnime(query: string)` method to `WatchSpaceService` (calls `GET /api/anilist/search?query=`)
- [x] 2.3 Add `addAnimeToWatchSpace(spaceId: string, body: AddAnimeToWatchSpaceRequest)` method to `WatchSpaceService` (calls `POST /watchspaces/{id}/anime`)

## 3. Anime Search Modal Component

- [x] 3.1 Create `anime-search-modal` component scaffold (`features/watch-spaces/anime-search-modal.ts` and `anime-search-modal.scss`) with `watchSpaceId` and `open` inputs, `closed` and `animeAdded` outputs
- [x] 3.2 Implement debounced search input (300ms) using signals and effect, wired to `WatchSpaceService.searchAnime()`
- [x] 3.3 Implement search results list with cover image, preferred title (English with romaji fallback), format badge, episode count, season/year, and genre badges (max 3)
- [x] 3.4 Implement loading, empty-state, and error-state views in the results area
- [x] 3.5 Implement per-result "Add" button that calls `WatchSpaceService.addAnimeToWatchSpace()`, with loading and success/error feedback per item
- [x] 3.6 Auto-focus the search input when the modal opens
- [x] 3.7 Style the modal content following the Kawaii/Y2K design system (tokens, BEM, bloom- prefix)
- [x] 3.8 Write unit tests for anime-search-modal (debounce, result rendering, add flow, error states)

## 4. Integration

- [x] 4.1 Add "Add Anime" button to `watch-space-detail` component that opens the anime search modal
- [x] 4.2 Wire the modal's `animeAdded` / `closed` events to refresh the watch space anime list when anime were added
- [x] 4.3 Write integration test for the full flow: open modal → search → add anime → close → list refreshes
