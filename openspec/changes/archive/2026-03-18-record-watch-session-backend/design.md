## Context

The AnimeTracking module supports adding anime (4.1), listing (4.2), detail (4.3), updating shared status (4.4), updating participant progress (4.5), and submitting ratings (4.6). The `WatchSpaceAnime` aggregate already owns a `_watchSessions` collection and the `WatchSession` entity, EF configuration, and `watch_sessions` table are all in place. What's missing is the domain creation logic, an application-layer use case, and a minimal API endpoint to record a session.

## Goals / Non-Goals

**Goals:**
- Expose `POST /watchspaces/{id}/anime/{watchSpaceAnimeId}/sessions` to create a watch session
- Add a domain factory/method on `WatchSpaceAnime` that validates episode range invariants and adds the session to its collection
- Follow established patterns: command/handler, membership check, domain exception → 400, not-found → 404
- Return 201 Created with the new session's details

**Non-Goals:**
- Editing or deleting watch sessions (future story)
- Listing sessions independently (already available via the anime detail endpoint)
- Updating `SharedEpisodesWatched` automatically from session data (that's managed separately via Story 4.4)
- Date validation beyond basic ISO 8601 parsing (future sessions are allowed per the acceptance criteria)

## Decisions

### 1. Add `RecordWatchSession` method on the aggregate

A new public method `RecordWatchSession(DateTime sessionDateUtc, int startEpisode, int endEpisode, string? notes, Guid createdByUserId)` validates the episode range, creates a `WatchSession`, and adds it to `_watchSessions`. This keeps creation logic encapsulated in the aggregate root, consistent with `UpdateParticipantProgress` and `UpdateParticipantRating`.

**Alternative considered:** A static factory on `WatchSession` itself called from the handler. Rejected — the session is owned by the aggregate, and the aggregate should control its own collection.

### 2. Validation lives in the domain method

The aggregate method enforces:
- `startEpisode >= 1`
- `endEpisode >= startEpisode`

A new `InvalidWatchSessionException` (extends `AnimeTrackingDomainException`) is thrown on violation. The endpoint maps this to 400 Bad Request, following the same pattern as `InvalidSharedStateException` and `InvalidParticipantProgressException`.

**Alternative considered:** Validate in the endpoint or use FluentValidation on the request DTO. Rejected — the codebase consistently places domain invariants in the domain layer and uses domain exceptions.

### 3. `WatchSession` gains an internal creation constructor

The `WatchSession` entity currently has only a private parameterless EF constructor. A new `internal` constructor (visible within the Domain assembly) will accept all fields and set them, including generating the `Id`. This is the same approach used by `ParticipantEntry`.

**Alternative considered:** A static `Create` factory method on `WatchSession`. Either approach works; constructor is slightly more concise for an entity with no complex creation logic.

### 4. New use case: `RecordWatchSession` command + handler

Follows the established pattern: command record, handler class, request DTO, result record. The handler loads the aggregate via `GetByIdAsync`, calls `RecordWatchSession`, then `SaveChangesAsync`. Returns a `RecordWatchSessionResult` with the session details.

### 5. Response is a flat session DTO, not the full aggregate

Unlike PATCH endpoints that return `GetWatchSpaceAnimeDetailResult`, this POST returns just the created session's fields (`id`, `sessionDateUtc`, `startEpisode`, `endEpisode`, `notes`, `createdByUserId`). The full aggregate is unnecessary — the caller knows which anime they posted to, and the session list is available via the detail endpoint.

**Alternative considered:** Return the full `GetWatchSpaceAnimeDetailResult`. Rejected — it over-fetches for a creation response, and the 201 pattern should return the created resource.

## Risks / Trade-offs

- **[Risk] No duplicate session detection** → Two identical POST requests create two sessions. Acceptable for MVP — sessions are append-only logs and users can view them on the detail page. Idempotency keys can be added later if needed.
- **[Risk] No upper-bound on episode range vs. episode count snapshot** → The story AC does not require `endEpisode <= episodeCountSnapshot`. This is intentional — ongoing/unknown-length anime may have no known episode count. Can tighten later if users request it.
