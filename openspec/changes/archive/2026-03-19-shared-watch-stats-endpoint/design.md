## Context

The Analytics module already exposes three endpoints (dashboard summary, compatibility score, rating gaps) that aggregate data from the `anime_tracking` schema. All three follow the same architecture: a minimal API endpoint delegates to a query handler, which checks membership via `IMembershipChecker`, loads data from `IWatchSpaceAnalyticsDataSource`, computes the result, and returns a typed DTO.

Story 5.4 adds a "shared watch stats" endpoint that aggregates counts and sums from `WatchSpaceAnime` records (episodes, finished/dropped status) and `WatchSession` records (session count, most recent date). The existing data source interface only returns anime-with-participants data; it has no method for watch sessions.

## Goals / Non-Goals

**Goals:**
- Expose `GET /watchspaces/{id}/analytics/shared-stats` returning five aggregate fields
- Follow the established analytics handler pattern exactly (membership check, data load, compute, return)
- Extend the analytics infrastructure to access watch session data

**Non-Goals:**
- Per-user stats breakdown (only shared/aggregate stats)
- Caching or performance optimization (data volumes are small per watch space)
- Pagination (response is a single flat object, not a list)
- Changing the dashboard endpoint (it already computes some overlapping stats independently)

## Decisions

### 1. Add a dedicated data-source method for watch session aggregates

**Choice:** Add a new method to `IWatchSpaceAnalyticsDataSource` that returns watch session count and most recent date, rather than loading all session entities.

**Rationale:** The endpoint only needs two aggregate values from `watch_sessions`. Loading full session entities would be wasteful. A targeted query returning `(int count, DateTime? mostRecentDate)` is simpler and more efficient.

**Alternative considered:** Reuse the existing `GetAnimeWithParticipantsAsync` and add sessions to `WatchSpaceAnimeData`. Rejected because it couples session data to the anime-participants projection and changes an interface used by three other handlers.

### 2. Compute anime-level aggregates from the existing data source

**Choice:** Reuse `GetAnimeWithParticipantsAsync` for `totalEpisodesWatchedTogether`, `totalFinished`, and `totalDropped` since it already returns `SharedStatus` and `SharedEpisodesWatched`.

**Rationale:** Avoids a second query or a new method for data already available. The dashboard handler already computes similar stats from this same data.

**Alternative considered:** A single raw SQL query returning all five fields. Rejected because it bypasses the existing abstraction and the data set per watch space is small enough that two queries are not a concern.

### 3. Follow the standard handler pattern with no new abstractions

**Choice:** Create `GetSharedStatsQuery`, `GetSharedStatsQueryHandler`, and `SharedStatsResult` under `UseCases/GetSharedStats/`, matching the exact patterns from Stories 5.1–5.3.

**Rationale:** Consistency. Every analytics use case follows this shape. No reason to deviate.

## Risks / Trade-offs

- **Overlapping data with dashboard**: `totalEpisodesWatchedTogether` and `totalFinished` are already computed in the dashboard endpoint. This is acceptable duplication — each endpoint serves a different purpose and decoupling them keeps handlers simple. → No mitigation needed.
- **Watch session table may be empty**: `mostRecentSessionDate` can be null. The result DTO handles this with a nullable `DateTime?`. → Handled by design.
- **Read DB context needs WatchSession projection**: The `AnimeTrackingReadDbContext` currently does not include a `WatchSessions` DbSet. This needs to be added for the session aggregate query. → Straightforward EF Core addition; read-only, no migrations needed since the table already exists.
