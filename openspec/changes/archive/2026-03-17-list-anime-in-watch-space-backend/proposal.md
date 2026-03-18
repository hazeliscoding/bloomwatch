## Why

Story 4.1 lets members add anime to a watch space, but there is no way to retrieve the list. Without a list endpoint, the frontend cannot render the shared anime backlog, currently-watching queue, or finished list — making the tracking feature incomplete.

## What Changes

- Add `GET /watchspaces/{id}/anime` endpoint returning all tracked anime in a watch space with shared status and participant summaries
- Support optional `?status=` query filter (backlog, watching, finished, paused, dropped)
- Add a query use-case (`ListWatchSpaceAnime`) following the existing CQRS pattern in the AnimeTracking module
- Extend the repository interface with a read method that supports optional status filtering
- Return results ordered by `addedAtUtc` descending

## Capabilities

### New Capabilities

- `list-watch-space-anime`: Read endpoint that lists all anime tracked in a watch space, with optional status filtering and participant entry summaries

### Modified Capabilities

_(none — the existing `add-anime-to-watch-space` spec is unaffected)_

## Impact

- **API:** New `GET /watchspaces/{id}/anime` endpoint added to `AnimeTrackingEndpoints`
- **Application layer:** New query, result DTO, and query handler in `AnimeTracking.Application`
- **Infrastructure:** New repository read method in `EfAnimeTrackingRepository`; query with eager-loaded `ParticipantEntry` data
- **No schema changes:** Reads from existing `anime_tracking.watch_space_anime` and `anime_tracking.participant_entries` tables
- **Cross-module dependency:** Reuses existing `IMembershipChecker` for the membership guard
