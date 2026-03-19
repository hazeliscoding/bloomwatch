## Why

Watch space members face decision paralysis when choosing what to watch from a long backlog. A random picker endpoint lets the frontend offer a "Pick for me" button that breaks the deadlock by selecting a random anime from the shared backlog — making the choosing step fun and instant. This is Story 5.5 in the MVP backlog (Epic 5 — Analytics and Dashboard).

## What Changes

- Add a new `GET /watchspaces/{id}/analytics/random-pick` endpoint in the Analytics module
- Implement a query handler that selects one anime at random from entries with `sharedStatus = Backlog` in the given watch space
- Return anime metadata including `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `mood`, `vibe`, and `pitch`
- Return 200 with `null` pick and a `"Backlog is empty"` message when no backlog items exist (not 404)
- Enforce membership authorization — non-members receive 403

## Capabilities

### New Capabilities

- `random-backlog-pick`: Server-side random selection of a single anime from a watch space's backlog, with metadata response and empty-backlog handling

### Modified Capabilities

_(none — this is a standalone read endpoint with no changes to existing specs)_

## Impact

- **Analytics module**: New query handler, DTO, and endpoint registration
- **AnimeTracking cross-module read**: Reuses the existing `AnimeTrackingReadDbContext` pattern already used by other analytics endpoints (dashboard-summary, compatibility, rating-gaps, shared-stats)
- **API surface**: One new GET endpoint added to the analytics route group
- **No breaking changes**: Additive only — no existing behavior is modified
