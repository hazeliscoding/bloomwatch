## Context

The Analytics module already has four read-only endpoints (dashboard, compatibility, rating-gaps, shared-stats) that follow an identical pattern: a query handler verifies membership via `IMembershipChecker`, loads data from the AnimeTracking schema via `IWatchSpaceAnalyticsDataSource`, computes results in-memory, and returns a result DTO. The new random backlog picker endpoint fits this exact pattern.

The existing `WatchSpaceAnimeData` DTO used by the analytics data source contains core fields (`WatchSpaceAnimeId`, `PreferredTitle`, `CoverImageUrl`, `EpisodeCountSnapshot`, `Format`, `SharedStatus`) but does **not** include `Mood`, `Vibe`, or `Pitch` — fields required by the Story 5.5 acceptance criteria. These fields exist on the `WatchSpaceAnime` domain aggregate and are persisted in the `anime_tracking.watch_space_anime` table.

## Goals / Non-Goals

**Goals:**

- Add a `GET /watchspaces/{id}/analytics/random-pick` endpoint that returns one randomly selected backlog anime
- Follow the established analytics module patterns exactly (query/handler/result + endpoint registration)
- Include `mood`, `vibe`, `pitch` fields in the response as specified in the acceptance criteria
- Return a graceful 200 with `null` pick and a message when the backlog is empty

**Non-Goals:**

- Weighted/algorithmic picking (mood-based, least-recently-suggested, etc.) — simple uniform random is sufficient
- Exclude-list or "don't show again" behavior — each call is independent
- Caching or deduplication of picks across calls
- Frontend implementation (Story 10.4 is a separate change)

## Decisions

### 1. Extend `WatchSpaceAnimeData` with `Mood`, `Vibe`, `Pitch` fields

**Decision:** Add `Mood`, `Vibe`, and `Pitch` as nullable string properties to the existing `WatchSpaceAnimeData` DTO and update the EF query in the data source implementation to project them.

**Rationale:** The random-pick response requires these fields per the acceptance criteria. Extending the shared DTO is simpler than creating a separate data source method. The dashboard's `BacklogHighlightResult` doesn't use these fields, so adding them to the DTO is additive with no impact on existing handlers.

**Alternative considered:** Create a separate `GetBacklogAnimeAsync()` method on `IWatchSpaceAnalyticsDataSource` that returns a new DTO with the extra fields. Rejected because it would duplicate the query logic and add unnecessary surface area for three nullable strings.

### 2. Randomization via `OrderBy(_ => Guid.NewGuid()).Take(1)` in application code

**Decision:** Load all backlog items from the data source (already loaded by the shared `GetAnimeWithParticipantsAsync` method), filter by `SharedStatus == "Backlog"`, shuffle with `OrderBy(_ => Guid.NewGuid())`, and take one.

**Rationale:** This matches the existing pattern used by `ComputeBacklogHighlights` in the dashboard handler. The data set is small (a watch space's backlog, typically <100 items), so in-memory randomization is efficient. Using database-level `ORDER BY RANDOM()` would require a new data source method and break the established pattern for minimal gain.

**Alternative considered:** PostgreSQL `ORDER BY RANDOM() LIMIT 1` for true server-side randomization. Rejected because the data is already loaded in-memory by the existing data source, and the acceptance criteria says "server-side" (which in-memory LINQ satisfies — it's not client/browser-side).

### 3. Wrapper result with nullable pick + message

**Decision:** Return a `RandomPickResult` record with `Pick` (nullable `RandomPickAnimeResult`) and `Message` (nullable string). When the backlog is empty, `Pick` is `null` and `Message` is `"Backlog is empty"`. When a pick is returned, `Message` is `null`.

**Rationale:** The acceptance criteria explicitly requires 200 with `null` and a message for empty backlogs (not 404). A wrapper object with both fields gives the frontend a clean contract to check.

### 4. Place in Analytics module following existing handler pattern

**Decision:** Create `GetRandomPick/` use case folder with `GetRandomPickQuery`, `RandomPickResult`, and `GetRandomPickQueryHandler` — identical structure to `GetSharedStats/`.

**Rationale:** Consistent with all four existing analytics use cases. No new architectural patterns needed.

## Risks / Trade-offs

- **[Risk] `GetAnimeWithParticipantsAsync` loads more data than needed** (participant entries, ratings) when only backlog metadata is required → Acceptable trade-off for a small dataset. If watch spaces grow very large, a dedicated query can be added later without API changes.
- **[Risk] `Guid.NewGuid()` randomization is pseudo-random, not cryptographically random** → Acceptable for a "pick something to watch" feature. Cryptographic randomness is not a requirement.
- **[Risk] Extending `WatchSpaceAnimeData` touches a shared DTO** → Low risk since the three new fields are nullable and additive. Existing consumers ignore fields they don't use.
