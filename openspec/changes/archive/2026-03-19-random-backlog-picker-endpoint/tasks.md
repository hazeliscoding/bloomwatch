## 1. Extend Shared Analytics DTO

- [x] 1.1 Add `Mood`, `Vibe`, and `Pitch` (all `string?`) to the `WatchSpaceAnimeData` record in `Analytics.Application/DTOs/WatchSpaceAnimeData.cs`
- [x] 1.2 Update the EF projection in the `IWatchSpaceAnalyticsDataSource` implementation (`Infrastructure/CrossModule/WatchSpaceAnalyticsDataSource.cs`) to include `Mood`, `Vibe`, `Pitch` from the `anime_tracking.watch_space_anime` table
- [x] 1.3 Verify existing analytics unit tests still pass after the DTO change (update test data builders if needed)

## 2. Query Handler and Result DTOs

- [x] 2.1 Create `GetRandomPickQuery` record with `WatchSpaceId` and `UserId` in `Analytics.Application/UseCases/GetRandomPick/`
- [x] 2.2 Create `RandomPickResult` record with `Pick` (`RandomPickAnimeResult?`) and `Message` (`string?`); create `RandomPickAnimeResult` record with `WatchSpaceAnimeId`, `PreferredTitle`, `CoverImageUrlSnapshot`, `EpisodeCountSnapshot`, `Mood`, `Vibe`, `Pitch`
- [x] 2.3 Implement `GetRandomPickQueryHandler` that: checks membership via `IMembershipChecker`, loads anime via `IWatchSpaceAnalyticsDataSource`, filters to `SharedStatus == "Backlog"`, selects one at random with `OrderBy(_ => Guid.NewGuid()).Take(1)`, returns `RandomPickResult` with null pick and `"Backlog is empty"` message when no backlog items exist

## 3. Endpoint Registration

- [x] 3.1 Add `GetRandomPickAsync` endpoint handler in `AnalyticsEndpoints.cs` following the existing pattern (extract user ID, call handler, catch `NotAWatchSpaceMemberException` → 403)
- [x] 3.2 Register the route as `group.MapGet("/analytics/random-pick", ...)` with `.WithName("GetRandomPick")`, `.Produces<RandomPickResult>(200)`, `.Produces(403)`
- [x] 3.3 Register `GetRandomPickQueryHandler` in the Analytics module's `ServiceCollectionExtensions`

## 4. Tests

- [x] 4.1 Add unit tests for `GetRandomPickQueryHandler`: non-member throws `NotAWatchSpaceMemberException`, empty backlog returns null pick with message, populated backlog returns a valid pick with all expected fields
- [x] 4.2 Add integration tests: `GET /watchspaces/{id}/analytics/random-pick` returns 200 with pick from seeded backlog, returns 200 with null pick when backlog is empty, returns 403 for non-member, returns 401 for unauthenticated request

## 5. Documentation

- [x] 5.1 Update `docs/user-stories.md` to mark Story 5.5 as ✅ Done
