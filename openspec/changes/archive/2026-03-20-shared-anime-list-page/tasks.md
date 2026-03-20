## 1. Model & Service Updates

- [x] 1.1 Add `AnimeParticipantSummary` interface to `watch-space.model.ts` with `userId`, `displayName`, `individualStatus`, `episodesWatched` fields
- [x] 1.2 Add `participants: AnimeParticipantSummary[]` field to the `WatchSpaceAnimeListItem` interface
- [x] 1.3 Update `WatchSpaceService.listWatchSpaceAnime()` to accept an optional `status` filter parameter and append `?status=` query param when provided

## 2. AnimeList Component

- [x] 2.1 Create `AnimeListComponent` standalone component in `features/watch-spaces/anime-list.ts` with inputs for `watchSpaceId` and a public `refresh()` method
- [x] 2.2 Implement data fetching: call `listWatchSpaceAnime()` on init and expose `animeList`, `isLoading`, `loadError` signals
- [x] 2.3 Implement status tab state: `activeTab` signal with values `all | backlog | watching | finished | paused | dropped`, default `all`; compute `filteredList` from `animeList` and `activeTab`
- [x] 2.4 Create the template with status tab bar, anime card grid, loading state, error state with retry, and per-tab empty states
- [x] 2.5 Render each anime card inside `<bloom-card>`: cover image, preferred title, shared status badge, shared episode progress (handle null `episodeCountSnapshot`), participant progress list
- [x] 2.6 Wire anime card click to navigate to `/watch-spaces/:id/anime/:watchSpaceAnimeId` via `Router`
- [x] 2.7 Add SCSS styles for the anime list layout, tab bar, card grid, and responsive breakpoints following BEM conventions

## 3. Integration with Watch Space Detail

- [x] 3.1 Import and render `<app-anime-list>` in `watch-space-detail.html` between the "Add Anime" section and the Members section, passing `watchSpaceId`
- [x] 3.2 Wire `onSearchModalClosedWithRefresh()` in `watch-space-detail.ts` to call `AnimeListComponent.refresh()` via a `ViewChild` ref
- [x] 3.3 Update `onSearchModalClosed()` to not trigger a refresh (no-op, modal dismissed without adding)

## 4. Testing

- [x] 4.1 Write unit tests for `AnimeListComponent`: default tab rendering, tab switching filters list, empty states per tab, loading and error states, card click navigation
- [x] 4.2 Write unit tests for the updated `WatchSpaceService.listWatchSpaceAnime()` method with and without status filter
- [x] 4.3 Verify integration: anime list refreshes after modal close with anime added, does not refresh on dismiss
