## Context

The AnimeTracking module supports individual progress tracking (Story 4.5) but participants cannot yet record their taste via a personal rating. The `ParticipantEntry` entity already has `RatingScore` and `RatingNotes` columns (added in the initial migration) but no domain method or API endpoint sets them. Story 4.6 introduces a dedicated PATCH endpoint for rating — separate from the progress endpoint because rating and progress have different validation rules and are independent user actions.

## Goals / Non-Goals

**Goals:**
- Expose `PATCH /watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-rating` accepting `{ ratingScore, ratingNotes? }`
- Add a domain method on `WatchSpaceAnime` that finds or creates the caller's `ParticipantEntry` and updates rating fields
- Validate the 0.5–10.0 scale in 0.5 increments at the domain level
- Follow established patterns: command/handler, membership check, domain exceptions, exception-to-HTTP mapping
- Return the updated `ParticipantDetail` so the frontend can update immediately

**Non-Goals:**
- Aggregating ratings across participants (compatibility analytics) — that is Story 9.x
- Updating progress fields (`episodesWatched`, `individualStatus`) — that is Story 4.5
- Allowing one user to rate on behalf of another
- Clearing/deleting a rating — out of scope; a user can overwrite but not remove

## Decisions

### 1. Add `UpdateParticipantRating` method on the aggregate

The `WatchSpaceAnime` aggregate root owns the `ParticipantEntry` collection, so the rating upsert method lives on the aggregate. Method signature: `ParticipantEntry UpdateParticipantRating(Guid userId, decimal ratingScore, string? ratingNotes)`. It finds the existing entry by `userId` or creates a new one, validates constraints, applies the update, and returns the entry.

**Alternative considered:** A separate `UpdateRating` method on `ParticipantEntry` called directly by the handler. Rejected — the aggregate should manage find-or-create logic to guard collection invariants (one entry per user), consistent with Story 4.5.

### 2. Domain-level rating scale validation

Validation of `ratingScore` (0.5–10.0, 0.5 increments) lives in the domain method, not the endpoint. A new `InvalidRatingException` (extends `AnimeTrackingDomainException`) is thrown for violations. The 0.5-increment check uses `ratingScore % 0.5m != 0` to catch values like 3.3 or 7.7.

**Alternative considered:** Validate at the endpoint/DTO level only. Rejected — domain rules belong in the domain; the endpoint layer should only catch and map exceptions.

### 3. Internal `UpdateRating` method on `ParticipantEntry`

Add `internal void UpdateRating(decimal ratingScore, string? ratingNotes)` on `ParticipantEntry` that sets `RatingScore`, `RatingNotes`, and `LastUpdatedAtUtc`. Marked `internal` so only the aggregate can call it, preserving encapsulation — same pattern as Story 4.5's `Update` method.

**Alternative considered:** Overload the existing `Update()` method to accept rating fields. Rejected — rating and progress are independent operations with different validation rules. Separate methods keep concerns clear.

### 4. Reuse the existing `Create` factory for upsert

Story 4.5 added `ParticipantEntry.Create(...)` for the upsert case. Reuse it with default progress values (`Backlog`, 0 episodes), then call `UpdateRating()` on the new entry. This avoids duplicating factory logic.

**Alternative considered:** Add a new factory overload that accepts rating fields. Rejected — the existing factory + `UpdateRating()` is straightforward and avoids proliferating factory methods.

### 5. Reuse `UpdateParticipantProgressResult` DTO as return type

Story 4.5's `UpdateParticipantProgressResult` already includes all `ParticipantDetail` fields (`userId`, `individualStatus`, `episodesWatched`, `ratingScore`, `ratingNotes`, `lastUpdatedAtUtc`). Rename it to a shared `ParticipantDetailResult` to avoid duplication.

**Alternative considered:** Create a separate `UpdateParticipantRatingResult`. Rejected — the shape is identical. A shared DTO reduces maintenance and aligns both endpoints on the same response contract. Renaming now (while only two consumers exist) is low-cost.

### 6. `ratingNotes` max-length validation at domain level

`ratingNotes` is validated to a max of 1000 characters in the domain method, consistent with the DB column constraint (`varchar(1000)`). The `InvalidRatingException` covers this case as well.

**Alternative considered:** Rely solely on the DB constraint to enforce length. Rejected — DB exceptions are harder to map to clean 400 responses; domain validation gives clear error messages.

## Risks / Trade-offs

- **[Risk] No concurrency control** → Same as Stories 4.4/4.5: last-write-wins. Since each user only updates their own entry, concurrent conflicts are unlikely. Mitigated by the unique index on `(watch_space_anime_id, user_id)`.
- **[Trade-off] Renaming the shared result DTO** → Touches Story 4.5's endpoint code. Low risk since the shape is unchanged — only the type name changes. If this feels risky, an alias or separate type is a fallback.
- **[Risk] Decimal precision edge cases** → `decimal` in C# handles 0.5 increments cleanly, but user input from JSON arrives as `double`. The `System.Text.Json` deserializer maps JSON numbers to `decimal` correctly for this range. No special handling needed.
