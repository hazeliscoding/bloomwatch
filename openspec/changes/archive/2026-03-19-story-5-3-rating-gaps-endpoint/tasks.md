## 1. New Use Case

- [x] 1.1 Create `GetRatingGapsQuery` record with `WatchSpaceId` and `UserId` fields
- [x] 1.2 Create `RatingGapsResult` and `RatingGapItem` response records (reuse existing `RaterResult` for per-rater data)
- [x] 1.3 Implement `GetRatingGapsQueryHandler` — check membership, load anime data, call `CompatibilityComputer.ComputeAnimeGaps`, apply secondary sort by title, resolve display names, return full list with message flag
- [x] 1.4 Register `GetRatingGapsQueryHandler` in `ServiceCollectionExtensions.AddAnalyticsModule`

## 2. API Endpoint

- [x] 2.1 Add `GET /watchspaces/{watchSpaceId:guid}/analytics/rating-gaps` route to `AnalyticsEndpoints` with handler method, producing 200/401/403
- [x] 2.2 Wire up ClaimsPrincipal extraction, query dispatch, and `NotAWatchSpaceMemberException` → 403 mapping

## 3. Testing

- [x] 3.1 Add unit test for secondary sort: equal gaps tie-broken by title alphabetically
- [x] 3.2 Add integration tests for `GET /watchspaces/{id}/analytics/rating-gaps`: 200 with data, 200 no qualifying data, 403 non-member, 401 unauthenticated

## 4. Docs

- [x] 4.1 Update `user-stories.md` to mark Story 5.3 as done
