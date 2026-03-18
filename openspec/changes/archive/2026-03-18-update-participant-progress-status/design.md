## Context

The AnimeTracking module supports shared anime state updates (Story 4.4) but individual participants cannot yet track their own progress. The `WatchSpaceAnime` aggregate owns a collection of `ParticipantEntry` entities — one per user — but after initial creation (Story 4.1), there is no method to update or create new entries. Story 4.5 introduces the first participant-level mutation: a PATCH endpoint that upserts the caller's personal progress.

A `ParticipantEntry` is automatically created for the user who adds an anime (Story 4.1). Other watch space members will not have entries until they interact — this endpoint creates one on first use (upsert).

## Goals / Non-Goals

**Goals:**
- Expose `PATCH /watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-progress` with required fields `episodesWatched` and `individualStatus`
- Add a domain method on `WatchSpaceAnime` that finds or creates the caller's `ParticipantEntry` and updates it
- Follow established patterns: command/handler, membership check, domain exceptions, exception-to-HTTP mapping
- Return the updated `ParticipantDetail` so the frontend can update immediately

**Non-Goals:**
- Updating rating fields (`ratingScore`, `ratingNotes`) — that is Story 4.6
- Recording watch sessions — that is Story 4.7
- Updating other users' participant entries — a user can only update their own
- Partial-update semantics — both fields are required on every call (unlike the shared status PATCH which uses nullable fields)

## Decisions

### 1. Add `UpdateParticipantProgress` method on the aggregate

The `WatchSpaceAnime` aggregate root owns the `ParticipantEntry` collection, so the upsert method lives on the aggregate. Method signature: `ParticipantEntry UpdateParticipantProgress(Guid userId, AnimeStatus individualStatus, int episodesWatched)`. It finds the existing entry by `userId` or creates a new one, validates constraints, applies the update, and returns the entry.

**Alternative considered:** Add an update method directly on `ParticipantEntry` and let the handler manage the find-or-create logic. Rejected — the aggregate should guard its own collection invariants (e.g., one entry per user).

### 2. Required fields (not nullable) for both parameters

Unlike Story 4.4's partial-patch semantics, both `episodesWatched` and `individualStatus` are always required. The user story specifies both fields in the request body, and there's no use case for updating one without the other. This simplifies validation — no null-checking needed.

**Alternative considered:** Nullable fields for partial-update consistency with Story 4.4. Rejected — the user story requires both fields, and partial updates add complexity without value here.

### 3. Domain exception for constraint violations

A new `InvalidParticipantProgressException` (extends `AnimeTrackingDomainException`) is thrown when:
- `episodesWatched` < 0
- `episodesWatched` > `EpisodeCountSnapshot` (when snapshot is known)

Invalid `individualStatus` enum values are caught at JSON deserialization (returns 400 automatically). The endpoint catches `AnimeTrackingDomainException` and maps to 400 Bad Request.

**Alternative considered:** Reuse `InvalidSharedStateException` from Story 4.4. Rejected — distinct exception types allow specific error messages and keep concerns separate.

### 4. Return `ParticipantDetail` (not the full anime detail)

The response returns only the updated participant entry as a `ParticipantDetail` DTO, not the full `GetWatchSpaceAnimeDetailResult`. The caller is updating their own progress and needs confirmation of that update — not the entire aggregate.

**Alternative considered:** Return `GetWatchSpaceAnimeDetailResult` for consistency with Story 4.4. Rejected — returning the full aggregate for a participant-level update is over-fetching and the user story specifies returning the participant entry.

### 5. Internal `Update` method on `ParticipantEntry`

Add an internal method `Update(AnimeStatus individualStatus, int episodesWatched)` on `ParticipantEntry` that sets the fields and updates `LastUpdatedAtUtc`. Marked `internal` so only the aggregate can call it, preserving encapsulation.

**Alternative considered:** Make setters public. Rejected — breaks DDD encapsulation pattern used throughout the codebase.

### 6. Internal `Create` factory on `ParticipantEntry` for upsert

Add an `internal static ParticipantEntry Create(WatchSpaceAnimeId watchSpaceAnimeId, Guid userId, AnimeStatus individualStatus, int episodesWatched)` factory to support the upsert case when no entry exists yet. The existing internal constructor covers creation at anime-add time (Backlog default); this new factory enables creation with caller-specified values.

**Alternative considered:** Reuse the existing constructor and then call `Update()`. Rejected — the existing constructor sets defaults (Backlog, 0 episodes). A dedicated factory is clearer and avoids a double-mutation.

## Risks / Trade-offs

- **[Risk] No concurrency control** → Same as Story 4.4: last-write-wins. Since each user only updates their own entry, concurrent conflicts are unlikely. Mitigated by the unique index on `(watch_space_anime_id, user_id)`.
- **[Risk] Unbounded entry creation** → Any watch space member can create their own entry. Bounded by watch space membership size (2–6 people). No mitigation needed.
