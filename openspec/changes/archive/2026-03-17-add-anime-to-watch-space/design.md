## Context

The AnimeTracking module does not exist yet. Story 4.1 is the first write operation for this module, introducing the `WatchSpaceAnime` aggregate root, `ParticipantEntry` entity, and the cross-module integration pattern for membership checks and AniList metadata lookups.

The codebase follows a modular monolith pattern with four sub-projects per module (Domain, Application, Infrastructure, Contracts). Existing modules (WatchSpaces, AniListSync, Identity) provide the patterns to follow. Cross-module communication uses application-layer abstractions (interfaces) backed by direct infrastructure-level calls within the same process.

**Constraints:**
- Each module owns its own EF Core DbContext and PostgreSQL schema
- Domain aggregates enforce invariants; application handlers orchestrate cross-module calls
- Strongly-typed IDs (value objects wrapping `Guid`) are used for aggregate identifiers
- The API layer uses minimal API endpoints registered in `BloomWatch.Api`

## Goals / Non-Goals

**Goals:**
- Stand up the full AnimeTracking module with Domain, Application, Infrastructure, and Contracts projects
- Implement `POST /watchspaces/{id}/anime` with membership verification, duplicate prevention, and metadata snapshotting
- Establish cross-module query patterns (WatchSpaces membership check, AniListSync media lookup) that subsequent Epic 4 stories can reuse
- Create the `anime_tracking` schema with initial EF Core migration

**Non-Goals:**
- Listing, updating, or deleting anime entries (Stories 4.2–4.7)
- Rating, watch session, or progress tracking (later stories build on this foundation)
- Real-time AniList API calls for cache-miss scenarios (the handler uses existing cached data or returns 404 if not cached — the frontend search flow in Story 3.1 ensures data is cached before this endpoint is called)
- Integration events for anime-added notifications (can be added later)

## Decisions

### 1. Cross-module membership verification via abstraction interface

**Decision:** Define `IMembershipChecker` in the AnimeTracking Application layer, implemented in Infrastructure by querying the WatchSpaces DbContext directly (same process, shared database).

**Rationale:** This follows the exact pattern of `IUserReadModel` in the WatchSpaces module. The AnimeTracking module does not take a dependency on WatchSpaces.Domain — it defines its own interface and the infrastructure project resolves it. This keeps the domain boundary clean while avoiding unnecessary network hops.

**Alternative considered:** Exposing a public API/service from WatchSpaces.Contracts. Rejected because the modules share the same database and process — an interface abstraction is sufficient and simpler.

### 2. AniList metadata lookup via abstraction interface

**Decision:** Define `IMediaCacheLookup` in the AnimeTracking Application layer, implemented in Infrastructure by querying the AniListSync DbContext's `media_cache` table directly.

**Rationale:** Same rationale as above. The AniListSync module already caches media data (Story 3.1 search + Story 3.2 detail). By the time a user hits "Add to Watch Space," the media is already cached from the search flow. The AnimeTracking module just reads the snapshot.

**Alternative considered:** Having the handler call the AniListSync Application layer's `GetMediaDetailQueryHandler` directly. Rejected because it couples to another module's internal application layer rather than using a clean abstraction.

### 3. Strongly-typed WatchSpaceAnimeId value object

**Decision:** Use `WatchSpaceAnimeId` as a `readonly record struct` wrapping `Guid`, consistent with `WatchSpaceId` in the WatchSpaces module.

**Rationale:** Type safety prevents accidental Guid mix-ups across aggregate boundaries.

### 4. Factory method on aggregate for creation

**Decision:** `WatchSpaceAnime.Create(...)` static factory method that constructs the aggregate and its initial `ParticipantEntry` atomically.

**Rationale:** Follows the established pattern from `WatchSpace.Create()`. The adding user's `ParticipantEntry` is part of the creation invariant — an anime cannot exist in a watch space without at least one participant.

### 5. Duplicate detection via unique constraint + repository check

**Decision:** Both a database-level unique constraint on `(watch_space_id, anilist_media_id)` and a repository method `ExistsAsync(watchSpaceId, anilistMediaId)` for the handler to check before creating.

**Rationale:** Belt-and-suspenders: the application layer provides a clean 409 response, the database prevents race conditions.

### 6. No real-time AniList API fallback

**Decision:** If the AniList media ID is not in the local cache, return 404 rather than fetching on-demand from AniList.

**Rationale:** The UI flow guarantees cache population — users search via Story 3.1 (which caches results) or view detail via Story 3.2 (which also caches). An on-demand fetch adds latency and error handling complexity for a scenario that shouldn't occur in normal usage.

## Risks / Trade-offs

- **[Stale metadata snapshots]** → Metadata is snapshotted at add-time and may drift from AniList. Acceptable for now; a background refresh job can be added later.
- **[Cross-schema read coupling]** → Infrastructure implementations read from other modules' schemas directly. This is a known trade-off of the modular monolith approach — if modules are ever extracted to separate services, these reads become API calls. Mitigation: all cross-module access goes through the abstraction interfaces.
- **[No cache-miss fallback]** → If somehow a media ID isn't cached, the endpoint returns 404. Mitigation: the UI workflow ensures caching happens during search; the 404 is a safety net, not an expected path.
