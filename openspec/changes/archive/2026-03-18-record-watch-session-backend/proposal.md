## Why

Watch spaces need a shared history of what was watched and when. While participant progress tracks individual episode counts, there is no way to log a group watch session with a specific episode range and date. This is the last piece of the anime tracking backend (Epic 4).

## What Changes

- Add a `POST /watchspaces/{id}/anime/{watchSpaceAnimeId}/sessions` endpoint that creates a `WatchSession` record
- Add domain logic on the `WatchSpaceAnime` aggregate to validate and create watch sessions (startEpisode >= 1, endEpisode >= startEpisode)
- Add a `RecordWatchSession` use case (command + handler) in the AnimeTracking Application layer
- The `WatchSession` entity and database table already exist — no schema changes needed

## Capabilities

### New Capabilities
- `record-watch-session`: Creating a watch session record for an anime in a watch space, with episode range validation, membership enforcement, and session date

### Modified Capabilities

## Impact

- **Module:** AnimeTracking (domain, application, API layers)
- **New endpoint:** `POST /watchspaces/{id}/anime/{watchSpaceAnimeId}/sessions`
- **Domain:** New factory method on `WatchSpaceAnime` aggregate and validation on `WatchSession` entity
- **No database migration required** — `watch_sessions` table and EF configuration already exist
