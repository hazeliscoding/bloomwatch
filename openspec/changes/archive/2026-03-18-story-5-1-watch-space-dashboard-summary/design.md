## Context

The AnimeTracking module handles all CRUD for watch space anime — adding, listing, detail, shared status, participant progress, ratings, and watch sessions (Stories 4.1–4.7). The frontend dashboard (Epic 9) needs a single endpoint that aggregates stats, lists, and compatibility data for a watch space, avoiding multiple round trips. This is the first endpoint in a new **Analytics** module (Epic 5), which will own all read-heavy aggregation queries across the AnimeTracking data.

## Goals / Non-Goals

**Goals:**
- Expose `GET /watchspaces/{id}/dashboard` as an authenticated, member-only endpoint returning a composite summary DTO
- Create the Analytics module skeleton (Domain, Application, Infrastructure, Contracts) following established modular monolith conventions
- Compute all summary data at read time from AnimeTracking data via a cross-module read abstraction
- Return stats, currently-watching list, backlog highlights, rating-gap highlights, and compatibility score in a single response

**Non-Goals:**
- Caching or pre-computing dashboard data (read-time aggregation is sufficient for MVP scale)
- Persisting analytics data to a dedicated `analytics` schema (no write model needed for this story)
- Pagination or filtering on the dashboard endpoint (fixed limits: 5 watching, 5 backlog, 3 rating gaps)
- Frontend implementation (separate Epic 9 story)
- Compatibility score as a standalone endpoint (Story 5.2 — this story embeds it in the dashboard response)

## Decisions

### 1. New Analytics module with cross-module read interface

The dashboard query reads from AnimeTracking data (anime, participant entries, ratings, watch sessions). Rather than querying the AnimeTracking module's repositories directly, the Analytics Application layer defines an `IWatchSpaceAnalyticsDataSource` interface. The Analytics Infrastructure layer implements it with direct database access to the `anime_tracking` schema (same-process, same-database — consistent with the `IMembershipChecker` pattern used by AnimeTracking to read from `watch_spaces`).

**Alternative considered:** Adding the dashboard endpoint to the AnimeTracking module. Rejected — it violates the module boundary. Analytics queries aggregate across tracking data but have different change reasons and will grow independently (Stories 5.2–5.5).

### 2. Single query handler, no domain aggregate

This is a pure read operation — there's no write model, no invariants to enforce, and no state to persist. The `GetDashboardSummaryQueryHandler` orchestrates the data retrieval via `IWatchSpaceAnalyticsDataSource` and assembles the result DTO directly. No Analytics domain aggregate is needed for this story.

**Alternative considered:** Creating an `AnalyticsSnapshot` aggregate that's computed and stored. Rejected — unnecessary complexity for MVP. The data is small (typical watch space has <50 anime) and the computation is lightweight.

### 3. Compatibility score computed inline

The compatibility score formula (`max(0, round(100 - averageGap × 10))`) is computed in the query handler from participant rating data. The per-anime gap is the average of absolute differences between all pairs of members' ratings. Only anime where 2+ members have rated are included. If no qualifying anime exist, compatibility returns `null` with a `"Not enough data"` message.

**Alternative considered:** Separate compatibility service or domain value object. Deferred — Story 5.2 will extract this into a standalone endpoint. For now, inline computation keeps things simple and avoids premature abstraction.

### 4. Backlog highlights use randomized selection

The spec requires "up to 5 randomly selected backlog items." The data source will return all backlog anime and the handler will select 5 at random. For testability, the randomization can use a seeded `Random` or be abstracted if needed, but for MVP a simple `OrderBy(x => Guid.NewGuid()).Take(5)` at the EF query level is sufficient.

**Alternative considered:** Database-level `ORDER BY RANDOM()`. This is effectively the same as `OrderBy(Guid.NewGuid())` in EF Core with PostgreSQL — both translate to random ordering.

### 5. Rating gap highlights use absolute difference across all member pairs

For each anime with 2+ ratings, the per-anime gap = mean of |ratingA - ratingB| across all distinct pairs of raters. The top 3 anime by gap are returned. Each highlight includes the anime info plus each member's rating.

### 6. No Analytics database schema or migrations for this story

Since the dashboard is purely a read-time computation with no persisted state, the Analytics module needs no `DbContext`, no schema, and no migrations. The Infrastructure layer only contains the `IWatchSpaceAnalyticsDataSource` implementation (which reads from `anime_tracking` schema) and the module registration.

**Alternative considered:** Creating an empty `analytics` schema for future use. Rejected — YAGNI. Add it when Stories 5.2+ actually need persisted data.

## Risks / Trade-offs

- **[Risk] N+1 query potential when computing rating gaps and compatibility** → Mitigate by loading all anime with participant entries in a single EF query (eager loading via `.Include()`), then computing in memory. Acceptable at MVP scale.
- **[Risk] Random backlog selection is non-deterministic** → This is by design per the user story. Integration tests should verify count and shape, not specific items.
- **[Risk] Cross-module data source couples Analytics Infrastructure to AnimeTracking's schema** → This is the established pattern (same as AnimeTracking → WatchSpaces). The interface abstraction means only Infrastructure knows about the schema coupling. If modules are later separated, the implementation swaps to an API call.
- **[Trade-off] Inline compatibility computation vs. reusable service** → Choosing inline for now. Story 5.2 will extract and potentially refactor. Slight duplication risk is acceptable to keep this story focused.
