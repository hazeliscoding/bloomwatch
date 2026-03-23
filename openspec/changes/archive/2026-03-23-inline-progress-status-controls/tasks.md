## 1. Service Layer

- [x] 1.1 Add `updateSharedAnime(spaceId, animeId, body)` method to `WatchSpaceService` that sends `PATCH /watchspaces/{spaceId}/anime/{animeId}` with optional `sharedStatus` and `sharedEpisodesWatched` fields
- [x] 1.2 Add request/response types for the shared anime update in `watch-space.model.ts`

## 2. Anime Detail Page — Wire Shared Controls

- [x] 2.1 Wire `onSharedStatusChange()` in `anime-detail.ts` to call `updateSharedAnime()` with optimistic update and rollback on error
- [x] 2.2 Wire `incrementSharedEpisode()` and `decrementSharedEpisode()` to call `updateSharedAnime()` with switchMap cancellation for rapid clicks
- [x] 2.3 Add inline error display below the shared status/episode section that auto-dismisses after 3 seconds
- [x] 2.4 Ensure the shared progress bar updates in sync with the episode stepper value

## 3. Anime List Page — Inline Status Dropdown

- [x] 3.1 Replace the read-only shared status badge on anime list cards with a `<select>` dropdown bound to `sharedStatus`
- [x] 3.2 Wire the dropdown's change event to call `updateSharedAnime()` with optimistic update and rollback
- [x] 3.3 Update tab counts after an inline status change without re-fetching the full list
- [x] 3.4 Add inline error display on the card for failed status changes

## 4. Anime List Page — Wire Participant Increment

- [x] 4.1 Handle the "+" button click in the anime list component by calling `updateParticipantProgress()` with `episodesWatched + 1`
- [x] 4.2 Restrict the "+" button visibility to the current user's own participant row only
- [x] 4.3 Disable the "+" button when episode count would exceed `episodeCountSnapshot`
- [x] 4.4 Add optimistic update for the participant row with rollback and inline error on failure

## 5. Styling and Polish

- [x] 5.1 Style the inline status dropdown on list cards to match the existing design system (kawaii/Y2K tokens)
- [x] 5.2 Style the inline error messages (small, below-control placement, fade-out animation)
- [x] 5.3 Verify reduced-motion support for error fade animations

## 6. Testing

- [x] 6.1 Add unit tests for `updateSharedAnime()` service method
- [x] 6.2 Add unit tests for anime detail shared status and episode wiring (optimistic update, rollback, switchMap)
- [x] 6.3 Add unit tests for anime list inline status dropdown and participant increment button
