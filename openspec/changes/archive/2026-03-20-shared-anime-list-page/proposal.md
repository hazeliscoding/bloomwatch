## Why

The watch space detail page currently manages settings, members, and invitations, and lets users add anime via the search modal (Story 9.1). However, there is no way to **browse** the anime that have been added. Members need a status-filtered list view so they can see what they're currently watching, what's in the backlog, and what's been finished — the core day-to-day surface of the app.

## What Changes

- Add a shared anime list view to the watch space detail page at `/watch-spaces/:id`
- Render status filter tabs: All, Backlog, Watching, Finished, Paused, Dropped
- Each anime card displays cover image, title, shared status, shared episode progress (e.g. "Ep 5 / 24"), and each member's individual episode count
- Clicking an anime card navigates to the anime detail page (`/watch-spaces/:id/anime/:watchSpaceAnimeId`)
- The existing "Add Anime" button opens the search modal; on close the list refreshes
- Empty states per status tab with friendly messaging
- Fetches from `GET /watchspaces/{id}/anime` with `?status=` filter parameter
- Extend the `WatchSpaceAnimeListItem` model to include participant progress data

## Capabilities

### New Capabilities
- `shared-anime-list`: The main anime list view component with status tabs, anime cards, loading/empty/error states, and navigation to detail page

### Modified Capabilities
- `list-watch-space-anime`: Extend to include participant data in the response model (the API already returns participants, but the frontend model needs to match)

## Impact

- **UI components**: New `AnimeListComponent` and `AnimeCardComponent` in `features/watch-spaces/`
- **Models**: `WatchSpaceAnimeListItem` in `watch-space.model.ts` needs a `participants` array field
- **Service**: `WatchSpaceService.listWatchSpaceAnime()` needs to accept an optional `status` filter parameter
- **Routing**: No new routes — the list renders inline on the existing watch space detail page
- **Existing code**: `watch-space-detail.ts` and its template will integrate the new list component
