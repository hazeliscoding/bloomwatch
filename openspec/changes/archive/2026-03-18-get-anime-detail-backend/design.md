## Context

The AnimeTracking module already supports adding anime to a watch space (Story 4.1) and listing all anime with participant summaries (Story 4.2). Both operate on the `WatchSpaceAnime` aggregate with its owned `ParticipantEntry` collection. The list endpoint returns a compact summary — it omits ratings, notes, and watch session history to keep the payload lightweight.

Story 4.3 adds a single-record detail endpoint that returns the full aggregate: all `WatchSpaceAnime` fields, full `ParticipantEntry` records (including `ratingScore`, `ratingNotes`, `lastUpdatedAtUtc`), and all `WatchSession` records.

The `WatchSession` entity does not yet exist in the domain. It will be introduced here as a read-only entity (the write path comes in Story 4.7), just enough to define the schema, EF configuration, and include it in the detail response.

## Goals / Non-Goals

**Goals:**
- Expose `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` with full aggregate data
- Introduce the `WatchSession` entity and its persistence so the schema is ready for Story 4.7
- Follow the same architectural patterns as the existing list endpoint (query object, handler, result DTO, membership check via `IMembershipChecker`)

**Non-Goals:**
- Write operations for `WatchSession` (Story 4.7)
- Resolving `displayName` for participants — the response returns `userId`; the frontend will resolve names from the membership list it already has
- Pagination or partial loading of participant entries or sessions

## Decisions

### 1. Introduce `WatchSession` entity now, write path later

The acceptance criteria require watch sessions in the response. Rather than stubbing the response with an empty list and adding the entity later, we introduce `WatchSession` as an owned entity of `WatchSpaceAnime` now. This avoids a future migration that alters the detail DTO shape.

**Alternative considered:** Return the detail without sessions, add sessions in Story 4.7. Rejected because the detail DTO contract would change mid-sprint, and the entity + table are trivial to add.

### 2. Single repository method with eager loading

Add `GetByIdAsync(Guid watchSpaceId, WatchSpaceAnimeId id)` to `IAnimeTrackingRepository`. The implementation uses `.Include(a => a.ParticipantEntries).Include(a => a.WatchSessions)` to load the full aggregate in one query. The `watchSpaceId` parameter scopes the lookup so a user cannot fetch anime from a space they haven't been membership-checked against.

**Alternative considered:** Separate repository calls for sessions. Rejected — the data set is small (few participants, few sessions) and a single query is simpler.

### 3. Result DTO mirrors the domain structure

`GetWatchSpaceAnimeDetailResult` includes nested `ParticipantDetail` and `WatchSessionDetail` records, mapping directly from the domain. Enums are serialized as strings (consistent with the list endpoint).

### 4. 404 when anime not found, 403 when not a member

Membership is checked first (same pattern as list endpoint — throw `NotAWatchSpaceMemberException`). If the anime does not exist in the given watch space, return 404. This ordering prevents information leakage about which anime exist in spaces the user doesn't belong to.

## Risks / Trade-offs

- **[Risk] WatchSession table created before write path exists** → Low impact. The table will be empty until Story 4.7. No data integrity concern since there's no write path yet.
- **[Risk] N+1 if EF generates suboptimal SQL for double Include** → Mitigated by verifying the generated SQL in integration tests. The data volume per aggregate is small (typically 2–6 participants, 0–20 sessions).
