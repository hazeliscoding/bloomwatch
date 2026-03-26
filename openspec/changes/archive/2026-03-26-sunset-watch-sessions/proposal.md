## Why

Watch sessions add friction without delivering proportional value. Users already track progress per-participant via episode counts and individual statuses, and shared status updates cover the group-level view. Requiring users to also log discrete watch sessions (date, episode range, notes) duplicates effort — especially since episode progress is the canonical source of "what's been watched." Sunsetting watch sessions now simplifies the UX and reduces code surface before the feature accumulates more data and dependencies.

## What Changes

- **BREAKING**: Remove the `POST /watchspaces/{id}/anime/{watchSpaceAnimeId}/sessions` endpoint
- **BREAKING**: Remove the `WatchSession` entity from the `WatchSpaceAnime` aggregate and its EF Core mapping
- **BREAKING**: Remove `totalWatchSessions` and `mostRecentSessionDate` fields from the shared-watch-stats response
- **BREAKING**: Remove watch session recording UI (form + modal) and session history display from the anime detail view
- **BREAKING**: Remove `watchSessions` array from the `GET /watchspaces/{id}/anime/{watchSpaceAnimeId}` detail response
- Remove `RecordWatchSession` command handler, request/result DTOs, and validator
- Remove analytics data source method `GetWatchSessionAggregateAsync` and its implementation
- Drop the `WatchSessions` database table via migration
- Remove associated unit and integration tests

## Capabilities

### New Capabilities

_None — this is a removal-only change._

### Modified Capabilities

- `watch-space-anime-detail`: Remove `watchSessions` from the detail response and the `recordWatchSession` service method
- `anime-detail-view`: Remove watch session history section and "Log Session" form/modal from the UI
- `shared-watch-stats`: Remove `totalWatchSessions` and `mostRecentSessionDate` from the stats response and computation
- `analytics-page`: Remove any watch-session-derived metrics from the analytics dashboard

## Impact

- **Domain**: `WatchSpaceAnime` aggregate loses `WatchSession` collection and `RecordWatchSession` method; `WatchSession` entity deleted
- **Application**: `RecordWatchSession` use-case folder removed entirely
- **API**: One endpoint removed (`POST .../sessions`); two response shapes shrink (detail + shared-stats)
- **UI**: Anime detail component loses session list and log-session modal; `WatchSessionDetail` interface removed from models; `recordWatchSession` method removed from service
- **Analytics module**: `IWatchSpaceAnalyticsDataSource.GetWatchSessionAggregateAsync` removed; analytics query/handler simplified
- **Database**: `WatchSessions` table dropped via new migration
- **Tests**: Unit and integration tests covering session recording, validation, and analytics aggregation removed
