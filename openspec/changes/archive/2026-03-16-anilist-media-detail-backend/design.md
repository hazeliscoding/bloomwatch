## Context

The AniListSync module currently supports search only (Story 3.1) — it proxies queries to the AniList GraphQL API and caches responses in memory for 5 minutes. There is no database persistence; the module's Domain project is empty.

Story 3.2 introduces a single-media detail endpoint with a **persistent database cache**. This is the first time AniListSync needs a `DbContext`, migrations, and an entity. The downstream consumer (Story 4.1 — Add Anime to Watch Space) will also use this cache to snapshot metadata when adding anime to watch spaces.

All existing patterns (handler-based use cases, `IAniListClient` abstraction, decorator caching, Minimal API endpoints, schema-per-module DbContext) are well-established in the codebase and will be followed.

## Goals / Non-Goals

**Goals:**

- Expose `GET /api/anilist/media/{anilistMediaId}` returning full cached metadata
- Persist fetched metadata in `anilist_sync.media_cache` with a 24-hour freshness window
- Extend `IAniListClient` with a `GetMediaByIdAsync` method backed by a new GraphQL query
- Follow all existing module patterns (layered architecture, handler DI, endpoint conventions)

**Non-Goals:**

- Changing existing search behavior or caching strategy (Story 3.1 is untouched)
- Background refresh of stale cache entries (on-demand fetch only)
- Bulk/batch media fetching
- Cache eviction policies beyond the 24-hour freshness check
- Frontend integration (separate change)

## Decisions

### 1. Cache entity lives in Domain, repository interface in Application

The `MediaCacheEntry` entity will be a simple domain class in the Domain project. The `IMediaCacheRepository` interface goes in Application (following the Identity module's `IUserRepository` pattern). The EF Core implementation goes in Infrastructure.

**Why over putting it all in Infrastructure:** Keeps the Application layer testable without EF Core dependencies. The cache entry is a first-class concept worth modelling — it will also be consumed by the AnimeTracking module (Story 4.1) for metadata snapshots.

### 2. Separate DbContext for AniListSync (`AniListSyncDbContext`)

Each module gets its own DbContext with its own schema (`anilist_sync`), matching the schema-per-module strategy used by Identity (`identity` schema) and WatchSpaces (`watch_spaces` schema).

**Why over sharing a DbContext:** Module isolation. Each module owns its schema and migrations independently. This is the established pattern.

### 3. Cache freshness check in the handler, not in the client

`GetMediaDetailQueryHandler` will:
1. Check `IMediaCacheRepository` for a cached entry
2. If fresh (within 24 hours), return it directly
3. If missing or stale, call `IAniListClient.GetMediaByIdAsync`
4. Persist the result via `IMediaCacheRepository`

**Why over putting cache logic in the client decorator:** The existing `CachedAniListClient` uses in-memory caching with `IMemoryCache`. The detail endpoint needs DB-backed caching with different semantics (24-hour freshness, persistence across restarts). Mixing both strategies in the decorator would conflate concerns. The handler owns the orchestration.

### 4. New `GetMediaByIdAsync` on `IAniListClient` returns a nullable DTO

`IAniListClient` gains: `Task<AnimeMediaDetail?> GetMediaByIdAsync(int anilistMediaId, CancellationToken)`.

Returns `null` when AniList returns no data for the ID (which maps to 404 at the endpoint). Throws `AniListApiException` on HTTP errors or malformed responses (maps to 502).

**Why nullable over throwing for not-found:** Not-found is a normal flow (user entered a bad ID), not an exceptional condition. Exceptions are reserved for upstream failures.

### 5. `AnimeMediaDetail` is a separate DTO, not extending `AnimeSearchResult`

A new `AnimeMediaDetail` record with all fields (superset of search). It's a distinct record rather than inheriting from `AnimeSearchResult` because:
- Records don't support inheritance well in C#
- The detail response includes `cachedAt` which is a caching concern, not an AniList concern
- Clean separation between search results and detail data

### 6. AniList failure must not corrupt existing cache

If the AniList call fails during a cache refresh (stale entry exists), the handler catches `AniListApiException` and returns 502 **without** deleting or updating the existing cache row. The stale entry remains available for the next request attempt.

**Why:** The user story explicitly requires this. A transient AniList outage should not destroy previously cached data.

## Risks / Trade-offs

- **[First DB migration for AniListSync]** → The module has never had a DbContext. This adds a design-time factory, a migration, and a schema. Mitigation: Follow the Identity module's exact pattern.
- **[24-hour stale window]** → A cached entry won't reflect AniList updates (e.g., episode count changes) for up to 24 hours. Mitigation: Acceptable for MVP. Background refresh can be added later if needed.
- **[No rate limiting on AniList calls]** → Each cache miss triggers an AniList API call. A malicious user could enumerate IDs. Mitigation: Auth is required (reduces attack surface). AniList itself rate-limits. Can add application-level throttling later.
- **[Single-row upsert concurrency]** → Two concurrent requests for the same uncached ID could both hit AniList and race on insert. Mitigation: Use upsert semantics (`UPDATE ... ON CONFLICT`) in the repository so the last write wins without errors.
