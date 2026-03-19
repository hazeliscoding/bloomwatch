## 1. Application Layer

- [x] 1.1 Create `GetSharedStatsQuery` record in `UseCases/GetSharedStats/`
- [x] 1.2 Create `SharedStatsResult` record with five fields (`TotalEpisodesWatchedTogether`, `TotalFinished`, `TotalDropped`, `TotalWatchSessions`, `MostRecentSessionDate`)
- [x] 1.3 Add `GetWatchSessionAggregateAsync` method to `IWatchSpaceAnalyticsDataSource` returning session count and most recent date
- [x] 1.4 Create `GetSharedStatsQueryHandler` — check membership, load anime data via existing method, load session aggregate via new method, compute and return result

## 2. Infrastructure Layer

- [x] 2.1 Add `WatchSessionReadModel` entity and `WatchSessions` DbSet to `AnimeTrackingReadDbContext`
- [x] 2.2 Implement `GetWatchSessionAggregateAsync` in `WatchSpaceAnalyticsDataSource` using the read context
- [x] 2.3 Register `GetSharedStatsQueryHandler` in `ServiceCollectionExtensions`

## 3. API Endpoint

- [x] 3.1 Add `GET /watchspaces/{watchSpaceId}/analytics/shared-stats` route to `AnalyticsEndpoints.cs` with membership-check error handling (403 Forbidden)

## 4. Unit Tests

- [x] 4.1 Test handler returns correct aggregates for a watch space with mixed anime statuses and sessions
- [x] 4.2 Test handler returns all-zero/null result for an empty watch space
- [x] 4.3 Test handler throws `NotAWatchSpaceMemberException` for non-members

## 5. Integration Tests

- [x] 5.1 Test endpoint returns 200 with correct stats for a seeded watch space
- [x] 5.2 Test endpoint returns 200 with zeroes/null for an empty watch space
- [x] 5.3 Test endpoint returns 403 for a non-member
- [x] 5.4 Test endpoint returns 401 for unauthenticated request

## 6. Documentation

- [x] 6.1 Update user stories doc to mark Story 5.4 as in-progress/done
