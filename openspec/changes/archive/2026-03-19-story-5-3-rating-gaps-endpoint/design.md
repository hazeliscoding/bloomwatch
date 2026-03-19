## Context

Story 5.2 extracted the shared compatibility/gap computation into `CompatibilityComputer` in the Analytics Application layer. The static method `ComputeAnimeGaps` already returns all anime with 2+ raters and their pairwise gaps sorted by gap descending. The dashboard handler caps results at 3 and resolves display names; this story exposes the full list.

Key differences from the dashboard's `ratingGapHighlights`:
- No cap — returns all qualifying anime
- Secondary sort by `preferredTitle` ascending for tie-breaking (dashboard has no tie-break)
- Includes a `message` field ("Not enough data") when no qualifying anime exist
- Response includes per-rater `displayName` and `ratingScore`

## Goals / Non-Goals

**Goals:**
- Expose `GET /watchspaces/{id}/analytics/rating-gaps` returning all anime with rating gaps
- Reuse `CompatibilityComputer.ComputeAnimeGaps` for computation
- Resolve display names via `IUserDisplayNameLookup`
- Return empty array with "Not enough data" message when no qualifying data

**Non-Goals:**
- Pagination (future concern — dataset is bounded by watch space anime count)
- Changing the gap computation formula
- Modifying existing dashboard or compatibility endpoints

## Decisions

### 1. New query handler calling shared computation

Create `GetRatingGapsQueryHandler` that:
1. Checks membership via `IMembershipChecker`
2. Loads anime data via `IWatchSpaceAnalyticsDataSource`
3. Calls `CompatibilityComputer.ComputeAnimeGaps` (returns sorted by gap descending)
4. Applies secondary sort: `.ThenBy(x => x.Anime.PreferredTitle)` for title-based tie-breaking
5. Resolves display names for all raters in a single batch
6. Returns the full list

**Why not add a parameter to the dashboard handler?** Same rationale as Story 5.2 — each endpoint should only compute what it needs, and the dashboard handles many more concerns.

### 2. Dedicated result DTOs

Create `RatingGapsResult` wrapping `IReadOnlyList<RatingGapItem>` and a nullable `string? Message`. Each `RatingGapItem` includes `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrl`, `gap`, and `ratings` (list of `{ userId, displayName, ratingScore }`). Reuse the existing `RaterResult` record from the dashboard DTOs.

### 3. Add route alongside existing analytics endpoints

Add to `AnalyticsEndpoints` under `/analytics/rating-gaps`, consistent with the `/analytics/compatibility` pattern from Story 5.2.

## Risks / Trade-offs

- **[Risk] Large watch spaces with many rated anime could return a big response** → Acceptable for MVP; the response is bounded by the number of anime in the space. Pagination can be added later if needed.
