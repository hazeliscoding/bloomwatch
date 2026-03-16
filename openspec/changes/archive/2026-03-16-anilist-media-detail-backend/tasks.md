## 1. Domain Layer

- [x] 1.1 Create `MediaCacheEntry` entity in `BloomWatch.Modules.AniListSync.Domain` with properties: `AnilistMediaId` (int, PK), `TitleRomaji`, `TitleEnglish`, `TitleNative`, `CoverImageUrl`, `Episodes`, `Status`, `Format`, `Season`, `SeasonYear`, `Genres` (list), `Description`, `AverageScore`, `Popularity`, `CachedAt` (DateTime UTC)

## 2. Application Layer

- [x] 2.1 Create `AnimeMediaDetail` record in `UseCases/GetMediaDetail/` with all response fields including `CachedAt`
- [x] 2.2 Add `GetMediaByIdAsync(int anilistMediaId, CancellationToken)` returning `AnimeMediaDetail?` to `IAniListClient`
- [x] 2.3 Create `IMediaCacheRepository` interface in `Abstractions/` with `GetByAnilistMediaIdAsync`, `UpsertAsync` methods
- [x] 2.4 Create `GetMediaDetailQuery` record and `GetMediaDetailQueryHandler` in `UseCases/GetMediaDetail/` — orchestrates cache check → AniList fetch → cache write, preserving stale entries on AniList failure

## 3. Infrastructure — AniList GraphQL Client

- [x] 3.1 Add `GetMediaById` GraphQL query string to `AniListGraphQlClient` requesting all detail fields (`id`, `title { romaji, english, native }`, `coverImage { large }`, `episodes`, `status`, `format`, `season`, `seasonYear`, `genres`, `description`, `averageScore`, `popularity`)
- [x] 3.2 Extend `AniListResponseModels` — add `Native` to `AniListTitle`, add `Description`, `AverageScore`, `Popularity` to `AniListMedia`
- [x] 3.3 Implement `GetMediaByIdAsync` in `AniListGraphQlClient` — POST the query, map response to `AnimeMediaDetail`, return `null` if media not found

## 4. Infrastructure — Persistence

- [x] 4.1 Create `AniListSyncDbContext` in `Infrastructure/Persistence/` with `anilist_sync` default schema and `DbSet<MediaCacheEntry>`
- [x] 4.2 Create `MediaCacheEntryConfiguration` (IEntityTypeConfiguration) mapping entity to `media_cache` table with snake_case columns and `Genres` stored as JSON
- [x] 4.3 Create `AniListSyncDbContextFactory` (IDesignTimeDbContextFactory) for EF Core tooling
- [x] 4.4 Generate initial EF Core migration for the `media_cache` table
- [x] 4.5 Implement `EfMediaCacheRepository` with `GetByAnilistMediaIdAsync` and `UpsertAsync` (using upsert semantics for concurrency safety)

## 5. DI Registration

- [x] 5.1 Update `AddAniListSyncModule` to accept `IConfiguration`, register `AniListSyncDbContext` with PostgreSQL, `IMediaCacheRepository`, and `GetMediaDetailQueryHandler`

## 6. API Endpoint

- [x] 6.1 Add `GET /api/anilist/media/{anilistMediaId:int}` to `AniListSyncEndpoints` — auth required, returns 200/404/502, delegates to `GetMediaDetailQueryHandler`
- [x] 6.2 Update `Program.cs` to pass `IConfiguration` to `AddAniListSyncModule` if not already

## 7. Tests

- [x] 7.1 Unit test `GetMediaDetailQueryHandler` — cache hit (fresh), cache miss (fetch + store), stale cache (refresh), AniList failure with stale entry (502, entry preserved), AniList returns null (404)
- [x] 7.2 Unit test `AniListGraphQlClient.GetMediaByIdAsync` — successful response mapping, null for unknown ID, exception on HTTP error
- [x] 7.3 Integration test for `GET /api/anilist/media/{id}` — 200 with valid ID, 404 with unknown ID, 401 without auth
