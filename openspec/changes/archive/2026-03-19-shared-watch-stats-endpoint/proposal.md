## Why

Watch space members want to see aggregate statistics about their shared watch history — total episodes, finished/dropped counts, session activity — so they can appreciate how much they've watched together. This is Story 5.4 in the analytics epic and fills the "shared stats" gap in the analytics suite (after dashboard summary, compatibility score, and rating gaps).

## What Changes

- Add a new `GET /watchspaces/{id}/analytics/shared-stats` endpoint (member-only, 403 for non-members)
- Return five aggregate fields computed from `WatchSpaceAnime` and `WatchSession` records:
  - `totalEpisodesWatchedTogether` — sum of `sharedEpisodesWatched` across all anime
  - `totalFinished` — count of anime with `sharedStatus = Finished`
  - `totalDropped` — count of anime with `sharedStatus = Dropped`
  - `totalWatchSessions` — count of recorded watch sessions
  - `mostRecentSessionDate` — date of the most recent watch session (null if none)
- Extend the analytics data source to expose watch session data for aggregation

## Capabilities

### New Capabilities
- `shared-watch-stats`: Aggregate statistics endpoint for shared watch history within a watch space

### Modified Capabilities
_(none — no existing spec-level requirements change)_

## Impact

- **API:** New route added to `AnalyticsEndpoints.cs`
- **Application:** New query, handler, and result DTO under `UseCases/GetSharedStats/`
- **Infrastructure:** `IWatchSpaceAnalyticsDataSource` may need a new method (or the read DB context extended) to query `WatchSession` counts and dates
- **Tests:** New unit tests for the handler and integration tests for the endpoint
- **Dependencies:** Reads from `anime_tracking` schema tables (`watch_space_anime`, `watch_sessions`)
