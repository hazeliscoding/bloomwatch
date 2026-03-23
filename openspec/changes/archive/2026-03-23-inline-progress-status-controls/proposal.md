## Why

The anime detail page has shared status and shared episode controls that only update local UI state — changes are lost on refresh because no backend PATCH calls are wired. The anime list page has a "+" button on participant rows that emits an event but nothing handles it. Story 9.4 requires low-friction inline updates from both the list and detail views so members can track progress without navigating away or losing changes.

## What Changes

- Wire the shared status dropdown on the anime detail page to `PATCH /watchspaces/{id}/anime/{animeId}` with `{ sharedStatus }`
- Wire the shared episode stepper on the anime detail page to `PATCH /watchspaces/{id}/anime/{animeId}` with `{ sharedEpisodesWatched }`
- Add a `updateSharedAnime()` service method to `WatchSpaceService` for the shared status/episodes PATCH endpoint
- Add an inline shared status dropdown on each anime list card that triggers the same PATCH
- Wire the existing "+" increment button on anime list participant rows to call `PATCH .../participant-progress` and update the card in-place
- Surface inline validation errors (e.g. episode count exceeding total) without disrupting the list or detail views
- Debounce or on-blur API calls for numeric inputs to avoid excessive requests

## Capabilities

### New Capabilities
- `inline-progress-controls`: Reusable inline episode increment and shared status update controls usable from both the anime list and detail views

### Modified Capabilities
- `shared-anime-list`: Adding inline status dropdown and wiring the participant "+" button on list cards
- `watch-space-anime-detail`: Wiring shared status dropdown and shared episode stepper to the backend PATCH endpoint

## Impact

- **Service layer**: New `updateSharedAnime()` method in `WatchSpaceService` (calls existing backend endpoint from Story 4.4)
- **Anime list component**: Template changes for inline status dropdown, event handling for "+" button, local state updates on success
- **Anime detail component**: Wire `onSharedStatusChange()`, `incrementSharedEpisode()`, `decrementSharedEpisode()` to service calls
- **No backend changes**: All endpoints already exist (Stories 4.4 and 4.5)
- **No new dependencies**: Uses existing Angular forms, service patterns, and UI components
