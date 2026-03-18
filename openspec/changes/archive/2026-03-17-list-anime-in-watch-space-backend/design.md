## Context

Story 4.1 established the AnimeTracking module with a `POST /watchspaces/{id}/anime` endpoint for adding anime. The module follows DDD/CQRS patterns: command handlers for writes, repository abstractions, cross-module read contexts for membership checks, and EF Core infrastructure. The domain model has `WatchSpaceAnime` as the aggregate root owning a collection of `ParticipantEntry` entities.

Story 4.2 adds the read side — a GET endpoint to list all tracked anime within a watch space, including each anime's participant summaries.

## Goals / Non-Goals

**Goals:**

- Implement `GET /watchspaces/{id}/anime` returning tracked anime with participant summaries
- Support optional `?status=` query parameter for filtering by shared status
- Follow the existing CQRS pattern: query DTO → query handler → result DTO
- Return results ordered by `addedAtUtc` descending
- Reuse the existing `IMembershipChecker` for the membership guard

**Non-Goals:**

- Pagination — watch spaces are expected to hold tens of anime, not thousands; pagination can be added as a non-breaking extension later
- Full-text search or user-configurable sort orders
- Response caching
- Any write-side changes or schema migrations

## Decisions

### 1. Query handler in the Application layer

Add a `ListWatchSpaceAnimeQuery` / `ListWatchSpaceAnimeQueryHandler` pair under `Application/UseCases/ListWatchSpaceAnime/`, mirroring how `AddAnimeToWatchSpaceCommandHandler` is structured.

**Rationale:** Keeps the endpoint thin (orchestration only) and the business logic testable in isolation. Consistent with the project's existing CQRS approach.

**Alternative considered:** Querying directly in the endpoint handler — rejected because it bypasses the application layer and couples HTTP concerns to data access.

### 2. Extend `IAnimeTrackingRepository` with a list method

Add `ListByWatchSpaceAsync(Guid watchSpaceId, AnimeStatus? statusFilter, CancellationToken)` returning `Task<IReadOnlyList<WatchSpaceAnime>>`. The EF Core implementation will eager-load `ParticipantEntries` via `.Include()`, apply the optional status filter, and order by `AddedAtUtc` descending.

**Rationale:** The repository already handles persistence for this aggregate. Adding a read method keeps the data access abstraction unified and avoids N+1 queries through eager loading.

**Alternative considered:** A separate read-only repository or a raw SQL projection — rejected for consistency with the established pattern and because the data volume doesn't warrant query optimization beyond `.Include()`.

### 3. Flat result DTO with nested participant summaries

`ListWatchSpaceAnimeResult` holds `IReadOnlyList<WatchSpaceAnimeListItem>`. Each item contains the anime's shared tracking fields plus `IReadOnlyList<ParticipantSummary>` (userId, individualStatus, episodesWatched).

**Rationale:** Matches the user story requirement to return participant summaries inline. Keeps the response shape flat enough for frontend consumption.

### 4. Enum query string binding for status filter

The `?status=` parameter binds to `AnimeStatus?`. ASP.NET Minimal APIs handle enum binding from query strings natively, returning 400 for invalid values automatically.

**Rationale:** Simple, type-safe, no custom binding logic needed.

## Risks / Trade-offs

- **[No pagination]** → Watch spaces are small (tens of entries). If usage grows, `?page=` / `?pageSize=` can be added as a non-breaking extension.
- **[Eager loading participant entries]** → Fetches all participant data per anime in a single query. Acceptable given the small expected size of watch spaces; avoids N+1 queries.
- **[No caching]** → Every request hits the database. Acceptable for MVP traffic levels; response caching can be added later.
