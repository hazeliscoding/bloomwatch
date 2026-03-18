## Context

The AnimeTracking module supports adding anime to a watch space (Story 4.1), listing anime (Story 4.2), and fetching full detail (Story 4.3). All three are read or create operations. The `WatchSpaceAnime` aggregate stores shared tracking fields (`SharedStatus`, `SharedEpisodesWatched`, `Mood`, `Vibe`, `Pitch`) but currently has no public mutation method — values are only set at creation time via `WatchSpaceAnime.Create()`.

Story 4.4 introduces the first mutation endpoint for the aggregate: a PATCH that lets any watch space member update the shared tracking fields with partial-update semantics.

## Goals / Non-Goals

**Goals:**
- Expose `PATCH /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` with partial-update semantics
- Add a domain mutation method on `WatchSpaceAnime` that enforces validation invariants
- Follow established architectural patterns: command/handler, membership check, exception-to-HTTP mapping
- Return the updated record so the frontend can refresh immediately without a follow-up GET

**Non-Goals:**
- Optimistic concurrency / conflict resolution (single-writer assumption; revisit if needed)
- Audit trail or change history for status transitions (future story)
- Updating individual participant entries (that's a separate use case)
- Updating media snapshot fields (`preferredTitle`, `coverImageUrlSnapshot`, etc.) — those come from AniList cache refresh

## Decisions

### 1. Add an `UpdateSharedState` method on the aggregate

Mutation logic lives on the `WatchSpaceAnime` aggregate root. A new public method `UpdateSharedState(AnimeStatus? sharedStatus, int? sharedEpisodesWatched, string? mood, string? vibe, string? pitch)` applies only the non-null parameters and enforces invariants before assignment.

**Alternative considered:** Set properties directly in the handler via public setters. Rejected — breaks the encapsulation pattern established by `Create()`, and validation would leak into the application layer.

### 2. Nullable parameters for partial-patch semantics

Each field in the command and request DTO is nullable. A `null` value means "do not change". The aggregate method checks each parameter for `null` before applying. This is the simplest approach for a small number of optional fields and avoids introducing a patch-document abstraction (JSON Patch, etc.).

**Alternative considered:** Use `JsonPatchDocument<T>` from `Microsoft.AspNetCore.JsonPatch`. Rejected — heavyweight for five flat fields, and the domain validation needs to happen inside the aggregate regardless.

### 3. Domain exception for constraint violations

A new `InvalidSharedStateException` (extends `AnimeTrackingDomainException`) is thrown when:
- `sharedEpisodesWatched` < 0
- `sharedEpisodesWatched` > `EpisodeCountSnapshot` (when snapshot is known)

The endpoint catches `AnimeTrackingDomainException` and maps it to 400 Bad Request with the exception message. Invalid `sharedStatus` enum values are caught at deserialization (returns 400 automatically via the JSON serializer's enum converter).

**Alternative considered:** Return a `Result<T>` from the domain method. Rejected — the codebase uses exceptions for domain rule violations consistently (e.g., `NotAWatchSpaceMemberException`, `AnimeAlreadyInWatchSpaceException`).

### 4. Reuse `GetWatchSpaceAnimeDetailResult` as the response DTO

The PATCH response returns the same shape as the GET detail endpoint. Rather than defining a separate result type, the handler maps the updated aggregate to `GetWatchSpaceAnimeDetailResult`. This keeps the frontend's type surface minimal and ensures PATCH and GET return identical shapes.

**Alternative considered:** A separate `UpdateSharedAnimeStatusResult` with just the updated fields. Rejected — the frontend needs the full record to update its state, and the aggregate is already loaded with all data.

### 5. Fetch-mutate-save via existing repository methods

The handler calls `GetByIdAsync()` to load the aggregate, invokes `UpdateSharedState()`, then calls `SaveChangesAsync()`. EF Core's change tracker handles the UPDATE statement. No new repository methods are needed.

## Risks / Trade-offs

- **[Risk] No concurrency control** → If two members update the same anime simultaneously, last-write-wins. Acceptable for a small-group collaborative tool. Mitigated by the fact that watch spaces are small (2–6 people). Can add optimistic concurrency (row version) later if needed.
- **[Risk] Returning full detail DTO from PATCH may over-fetch** → The handler eagerly loads participant entries and watch sessions just to build the response. Acceptable given the small data volume per aggregate. If this becomes a concern, a lightweight result DTO can be introduced later.
