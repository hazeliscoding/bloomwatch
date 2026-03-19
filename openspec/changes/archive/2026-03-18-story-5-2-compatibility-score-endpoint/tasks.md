## 1. Extract Shared Computation

- [x] 1.1 Create `CompatibilityComputer` static class in Analytics Application layer (`Shared/CompatibilityComputer.cs`) and move `ComputeAnimeGaps`, `ComputePairwiseGap`, `ComputeCompatibility`, and `GetCompatibilityLabel` from `GetDashboardSummaryQueryHandler` into it
- [x] 1.2 Update `GetDashboardSummaryQueryHandler` to call `CompatibilityComputer` instead of its own methods — verify existing unit tests still pass

## 2. New Use Case

- [x] 2.1 Create `GetCompatibilityQuery` record with `WatchSpaceId` and `UserId` fields
- [x] 2.2 Create `CompatibilityScoreResult` response record wrapping `CompatibilityResult?` and `string? Message`
- [x] 2.3 Implement `GetCompatibilityQueryHandler` — check membership, load anime data, delegate to `CompatibilityComputer`, return `CompatibilityScoreResult`
- [x] 2.4 Register `GetCompatibilityQueryHandler` in `ServiceCollectionExtensions.AddAnalyticsModule`

## 3. API Endpoint

- [x] 3.1 Add `GET /watchspaces/{watchSpaceId:guid}/analytics/compatibility` route to `AnalyticsEndpoints` with handler method, producing 200/401/403
- [x] 3.2 Wire up ClaimsPrincipal extraction, query dispatch, and `NotAWatchSpaceMemberException` → 403 mapping

## 4. Testing

- [x] 4.1 Update existing `CompatibilityScoreTests` and `RatingGapComputationTests` to reference `CompatibilityComputer` instead of `GetDashboardSummaryQueryHandler`
- [x] 4.2 Add integration tests for `GET /watchspaces/{id}/analytics/compatibility`: 200 with data, 200 insufficient data, 403 non-member, 401 unauthenticated

## 5. Docs

- [x] 5.1 Update `user-stories.md` to mark Story 5.2 as done
