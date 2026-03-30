## 1. AniList GraphQL and Cache Extension

- [x] 1.1 Extend the AniList `MediaByIdQuery` GraphQL string to include `tags { name rank isMediaSpoiler }` and `siteUrl`
- [x] 1.2 Add `MediaTag` record (Name, Rank, IsMediaSpoiler) to the AniListSync domain and update `AnimeMediaDetail` DTO to include `Tags` (list) and `SiteUrl` (string)
- [x] 1.3 Update `AniListGraphQlClient` response deserialization and mapping to populate tags and siteUrl on `AnimeMediaDetail`
- [x] 1.4 Add `Tags` (IReadOnlyList<MediaTag>) and `SiteUrl` (string?) properties to `MediaCacheEntry`, update `Create()` and `Update()` methods
- [x] 1.5 Update EF Core `MediaCacheEntryConfiguration` to map Tags as JSONB and SiteUrl as text column
- [x] 1.6 Add EF Core migration for the new Tags and SiteUrl columns on `anilist_sync.media_cache`
- [x] 1.7 Update `GetMediaDetailQueryHandler` to pass tags and siteUrl through the cache create/update flow

## 2. Cross-Module Enrichment (AnimeTracking)

- [x] 2.1 Define `IMediaCacheReader` adapter interface in AnimeTracking.Application.Abstractions with a `GetMediaCacheAsync` method returning an enrichment DTO
- [x] 2.2 Implement `MediaCacheReaderAdapter` in AnimeTracking.Infrastructure that queries `anilist_sync.media_cache` by anilistMediaId
- [x] 2.3 Register the adapter in the AnimeTracking module's DI setup
- [x] 2.4 Extend `GetWatchSpaceAnimeDetailResult` record with nullable fields: Genres, Description, AverageScore, Popularity, Tags, SiteUrl, AiringStatus
- [x] 2.5 Update `GetWatchSpaceAnimeDetailQueryHandler` to inject `IMediaCacheReader`, call it after loading the aggregate, and populate the enrichment fields on the result

## 3. Frontend Model and UI

- [x] 3.1 Add `AnimeTag` interface and update `WatchSpaceAnimeDetail` interface in `watch-space.model.ts` with new fields: `tags`, `siteUrl`, `airingStatus`
- [x] 3.2 Add AniList external link to the hero section (clickable "View on AniList" with fallback URL construction)
- [x] 3.3 Add airing status badge to the meta line using `bloom-badge` with status-appropriate colors
- [x] 3.4 Add tags display section below genres with `bloom-badge` components, sorted by rank, limited to 15
- [x] 3.5 Implement spoiler tag hiding: blurred badges with click-to-reveal interaction
- [x] 3.6 Switch description rendering from `{{ }}` interpolation to `[innerHTML]` with Angular DomSanitizer pipe for safe HTML
- [x] 3.7 Add SCSS styles for new elements: tags section, spoiler tags, external link, airing status badge

## 4. Tests

- [x] 4.1 Update AniListSync unit tests for the extended GraphQL response mapping and cache entry with tags/siteUrl (N/A — no backend test project exists)
- [x] 4.2 Update AnimeTracking unit tests for the enriched detail query handler (with and without media cache hit) (N/A — no backend test project exists)
- [x] 4.3 Update AnimeTracking integration tests for the extended detail endpoint response shape (N/A — no backend test project exists)
- [x] 4.4 Add frontend tests for tags display, spoiler toggle, external link rendering, airing status badge, and HTML description rendering
