## Why

The frontend needs full anime metadata to render a detail page (Story 3.2). The existing search endpoint (Story 3.1) returns only summary fields. A dedicated detail endpoint is needed that returns the complete metadata set for a single AniList media entry, backed by a persistent database cache so repeated lookups don't hammer the AniList API and survive application restarts.

## What Changes

- Add `GET /api/anilist/media/{anilistMediaId:int}` endpoint returning full cached metadata for a single anime
- Introduce a persistent `anilist_sync.media_cache` database table with 24-hour freshness window
- Extend `IAniListClient` with a `GetMediaByIdAsync` method using a new single-media GraphQL query
- Add new Application use case (`GetMediaDetailQuery` / `GetMediaDetailQueryHandler`) following existing patterns
- Return additional fields not present in search results: `description`, `averageScore`, `popularity`, `titleNative`
- Return `cachedAt` timestamp in the response so the frontend knows data freshness
- Return 404 when AniList doesn't know the requested media ID
- Return 502 on AniList API failures without corrupting existing cache entries

## Capabilities

### New Capabilities

- `anilist-media-detail`: Fetching, caching, and serving full metadata for a single AniList media entry by ID

### Modified Capabilities

_(none — search behavior is unchanged)_

## Impact

- **AniListSync module**: New domain entity (`MediaCacheEntry`), new application use case + DTO, new `IAniListClient` method, new GraphQL query, new EF Core `DbContext` + migration, extended DI registration
- **API layer**: New endpoint added to `AniListSyncEndpoints` under existing `/api/anilist` route group
- **Database**: New `anilist_sync.media_cache` table (first DB usage for the AniListSync module)
- **Dependencies**: No new external dependencies — uses existing EF Core and `HttpClient` infrastructure
