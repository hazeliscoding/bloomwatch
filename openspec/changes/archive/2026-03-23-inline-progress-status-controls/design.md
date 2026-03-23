## Context

The anime detail page (Story 9.3) ships with shared status dropdown and shared episode stepper controls that update local signal state only — no backend calls are wired. The anime list page has a "+" button per participant that emits an `incrementEpisode` output event, but no parent component handles it. Story 9.4 requires wiring these controls to the existing backend endpoints so changes persist.

All backend endpoints already exist:
- `PATCH /watchspaces/{id}/anime/{animeId}` (Story 4.4) — accepts `{ sharedStatus?, sharedEpisodesWatched? }`
- `PATCH /watchspaces/{id}/anime/{animeId}/participant-progress` (Story 4.5) — accepts `{ individualStatus, episodesWatched }`

The Angular service (`WatchSpaceService`) has methods for participant progress and rating but lacks a method for the shared anime PATCH.

## Goals / Non-Goals

**Goals:**
- Wire shared status and shared episode controls on the detail page to persist via PATCH
- Wire the "+" participant increment button on the list page to persist via PATCH
- Add an inline shared status dropdown on anime list cards
- Add a service method for the shared anime PATCH endpoint
- Show inline validation errors without disrupting the surrounding view
- Provide immediate UI feedback (optimistic update with rollback on failure)

**Non-Goals:**
- Mood/vibe/pitch editing (separate concern, not part of Story 9.4)
- Batch/bulk status updates across multiple anime
- Drag-and-drop reordering or Kanban board views
- Real-time sync between multiple users viewing the same list

## Decisions

### 1. Single `updateSharedAnime()` service method with partial body

Add one method to `WatchSpaceService`:
```typescript
updateSharedAnime(spaceId: string, animeId: string, body: { sharedStatus?: string; sharedEpisodesWatched?: number }): Observable<WatchSpaceAnimeDetail>
```

**Rationale:** The backend endpoint already accepts partial updates. A single method with optional fields mirrors the API contract and avoids unnecessary method proliferation. Both the list and detail pages call the same method.

**Alternative considered:** Separate `updateSharedStatus()` and `updateSharedEpisodes()` methods — rejected because they would call the same endpoint and add no value.

### 2. Optimistic UI updates with rollback on error

When a user changes a shared status or increments episodes, update the local signal immediately and fire the API call. On failure, revert the signal to the previous value and show an inline error.

**Rationale:** The controls feel instant (no spinner for a single field change). The revert pattern is simple since we snapshot the previous value before mutating. Matches the existing participant progress pattern in the detail component.

**Alternative considered:** Wait for API response before updating UI — rejected because it makes controls feel sluggish for simple increment/status changes.

### 3. Inline status dropdown on anime list cards

Add a `<select>` element to each anime card in the list view, bound to `sharedStatus`. On change, call `updateSharedAnime()` and update the card's local data. Position it near the existing status badge location, replacing the read-only badge for members.

**Rationale:** Story 9.4 acceptance criteria explicitly requires updating shared status from the list view without navigating away. A native `<select>` is the simplest control that matches the detail page's existing dropdown pattern.

### 4. Handle the "+" button via parent component in the watch space detail page

The anime list component emits `incrementEpisode` events. The parent watch space detail component will handle this event by calling `WatchSpaceService.updateParticipantProgress()` with `episodesWatched + 1`. On success, refresh the specific card's participant data.

**Rationale:** The list component already emits the event — the wiring just needs a handler. Keeping the API call in the parent (or the list component itself) avoids passing the service into each card. The list component already has access to `WatchSpaceService`.

**Refined approach:** Move the increment handling into the anime list component itself rather than emitting to parent, since the list component already injects the service and has the data context. This avoids the need for the parent to manage anime list state.

### 5. Debounce strategy for episode stepper

The episode stepper (detail page) fires on each click of +/−. Rather than debouncing (which would delay feedback), fire the API call immediately on each click but use a per-anime request queue that cancels in-flight requests when a new one arrives (switchMap pattern).

**Rationale:** Episode changes are discrete actions (click +, click +), not continuous input. Debouncing would mean the UI shows a value the backend doesn't know about yet. SwitchMap cancellation ensures only the latest value persists while keeping the UI responsive.

### 6. Inline error display

Show validation errors (e.g. "Cannot exceed 24 episodes") as a small toast-like message below the control that fades after 3 seconds. Do not use modal dialogs or page-level alerts for inline control errors.

**Rationale:** Inline errors should be proportional to the action. A tiny status change shouldn't produce a modal. The fade-after-3s pattern prevents error messages from accumulating on the page.

## Risks / Trade-offs

- **Race condition on shared state**: Two members can update shared status simultaneously, last-write-wins. → Acceptable for MVP; the backend returns the updated record so the UI will reflect the latest state after refresh.
- **Stale list data after detail page changes**: If a user updates shared status on the detail page and navigates back, the list may show stale data. → Mitigated by re-fetching the anime list on component init (already the current behavior).
- **Optimistic rollback flicker**: If the API fails, the UI briefly shows the new value then reverts. → Acceptable UX; the error message explains what happened.
