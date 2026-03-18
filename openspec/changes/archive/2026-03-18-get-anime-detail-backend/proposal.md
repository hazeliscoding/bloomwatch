## Why

The anime list endpoint (Story 4.2) returns summary data for all anime in a watch space, but users need a detail view to see the full picture for a single anime — including every participant's rating/notes and the watch session history. Story 4.3 adds the backend endpoint that powers this detail screen.

## What Changes

- Add `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` endpoint returning the full aggregate for a single tracked anime
- Extend the response to include full `ParticipantEntry` records (with ratings and notes) beyond the summary returned by the list endpoint
- Include `WatchSession` records in the response (session history for this anime in the watch space)
- Add a `WatchSession` domain entity to the AnimeTracking module (does not yet exist)
- Add a `GetByIdAsync` repository method to retrieve a single `WatchSpaceAnime` with all related data
- Enforce membership check (403) and not-found handling (404)

## Capabilities

### New Capabilities
- `watch-space-anime-detail`: Single-anime detail endpoint returning full aggregate with participant entries (including ratings) and watch session list

### Modified Capabilities
_(none — the list endpoint and its spec are unchanged)_

## Impact

- **API**: New `GET` endpoint on the existing `/watchspaces/{watchSpaceId}/anime` route group
- **Domain**: New `WatchSession` entity added to the `WatchSpaceAnime` aggregate; aggregate gains a `WatchSessions` collection
- **Persistence**: New `watch_sessions` table + EF Core configuration; migration required; new repository method
- **No breaking changes** to existing endpoints or contracts
