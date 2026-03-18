## 1. Analytics Module Scaffold

- [x] 1.1 Create `BloomWatch.Modules.Analytics.Contracts` class library project with module marker interface
- [x] 1.2 Create `BloomWatch.Modules.Analytics.Domain` class library project (empty for now — no domain aggregates needed for this story)
- [x] 1.3 Create `BloomWatch.Modules.Analytics.Application` class library project with project references to Domain and Contracts
- [x] 1.4 Create `BloomWatch.Modules.Analytics.Infrastructure` class library project with project references to Application and Domain
- [x] 1.5 Add all four projects to the solution file and wire up project references from `BloomWatch.Api` to Analytics Infrastructure and Contracts
- [x] 1.6 Create `AnalyticsModuleExtensions` in Infrastructure with `AddAnalyticsModule` (DI registration) and `MapAnalyticsEndpoints` (endpoint mapping) methods
- [x] 1.7 Register the Analytics module in `Program.cs` / API host startup

## 2. Cross-Module Data Abstraction

- [x] 2.1 Define `IWatchSpaceAnalyticsDataSource` interface in Analytics Application layer with methods to retrieve anime with participant entries and ratings for a given watch space
- [x] 2.2 Define `IMembershipChecker` interface in Analytics Application layer (or reuse existing pattern) for verifying watch space membership
- [x] 2.3 Create DTOs in Analytics Application layer for the data returned by `IWatchSpaceAnalyticsDataSource` (anime summary, participant rating, etc.)

## 3. Infrastructure — Cross-Module Read Implementation

- [x] 3.1 Implement `WatchSpaceAnalyticsDataSource` in Analytics Infrastructure that queries the `anime_tracking` schema via EF Core (using the existing `AnimeTrackingDbContext` or a read-only DbContext) to load anime with participant entries
- [x] 3.2 Implement `MembershipChecker` in Analytics Infrastructure that queries the `watch_spaces` schema to verify membership (following the pattern from AnimeTracking module)
- [x] 3.3 Register both implementations in `AnalyticsModuleExtensions.AddAnalyticsModule`

## 4. Application — Dashboard Query Handler

- [x] 4.1 Create `GetDashboardSummaryQuery` record with `WatchSpaceId` and `UserId` fields
- [x] 4.2 Create result DTOs: `DashboardSummaryResult`, `DashboardStatsResult`, `CompatibilityResult`, `CurrentlyWatchingItemResult`, `BacklogHighlightResult`, `RatingGapHighlightResult`, `RaterResult`
- [x] 4.3 Implement `GetDashboardSummaryQueryHandler` — check membership, load all anime data, then compute and assemble the response
- [x] 4.4 Implement stats computation: `totalShows`, `currentlyWatching` count, `finished` count, `episodesWatchedTogether` sum
- [x] 4.5 Implement currently-watching list: filter by `Watching` status, order by `AddedAtUtc` descending, take 5
- [x] 4.6 Implement backlog highlights: filter by `Backlog` status, random selection, take 5
- [x] 4.7 Implement rating gap computation: for each anime with 2+ raters, compute mean of absolute pairwise differences, sort descending, take 3
- [x] 4.8 Implement compatibility score: `max(0, round(100 - averageGap × 10))` with label mapping, return null when no anime has 2+ raters

## 5. API Endpoint

- [x] 5.1 Create `AnalyticsEndpoints` class with `MapAnalyticsEndpoints` method
- [x] 5.2 Add `GET /watchspaces/{watchSpaceId}/dashboard` route that maps to a `GetDashboardSummaryAsync` handler method
- [x] 5.3 Wire up ClaimsPrincipal extraction, query dispatch, exception handling (403 for non-member, 404 for not-found), and return `200 OK` with the summary DTO

## 6. Testing

- [x] 6.1 Add unit tests for compatibility score computation: perfect score, high gap, zero clamp, rounding
- [x] 6.2 Add unit tests for rating gap computation: 2 raters, 3+ raters, no qualifying anime
- [x] 6.3 Add unit tests for compatibility label mapping across all score ranges
- [x] 6.4 Add integration tests for the GET dashboard endpoint: 200 with full data, 200 empty watch space, 403 non-member, 404 not found, 401 unauthenticated

## 7. Docs

- [x] 7.1 Update user-stories.md to mark Story 5.1 as done and update sprint progress
