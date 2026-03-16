## 1. Module Scaffolding

- [x] 1.1 Create the AniListSync module directory structure with four projects: Domain, Application, Infrastructure, Contracts
- [x] 1.2 Create `.csproj` files for each project with appropriate dependencies (match Identity module patterns)
- [x] 1.3 Add all four project references to the solution file
- [x] 1.4 Add project reference from `BloomWatch.Api` to `BloomWatch.Modules.AniListSync.Infrastructure`

## 2. Domain & Application Layer

- [x] 2.1 Create `AnimeSearchResult` DTO in the Application layer with all required fields (`anilistMediaId`, `titleRomaji`, `titleEnglish`, `coverImageUrl`, `episodes`, `status`, `format`, `season`, `seasonYear`, `genres`)
- [x] 2.2 Define `IAniListClient` interface in the Application layer with `SearchAnimeAsync(string query, CancellationToken ct)` returning a list of `AnimeSearchResult`
- [x] 2.3 Implement `SearchAnimeQuery` and `SearchAnimeQueryHandler` in the Application layer, wiring `IAniListClient` with input validation (reject null/empty/whitespace query)

## 3. Infrastructure Layer — AniList GraphQL Client

- [x] 3.1 Implement `AniListGraphQlClient : IAniListClient` using typed `HttpClient` that sends the search GraphQL query to `https://graphql.anilist.co`
- [x] 3.2 Define the static GraphQL search query string requesting all required media fields
- [x] 3.3 Create response deserialization models for the AniList GraphQL response shape
- [x] 3.4 Map AniList response models to `AnimeSearchResult` DTOs

## 4. Infrastructure Layer — Caching & Registration

- [x] 4.1 Implement a caching decorator or wrapper around `IAniListClient` using `IMemoryCache` with 5-minute absolute expiration and case-insensitive query normalization
- [x] 4.2 Create `ServiceCollectionExtensions.AddAniListSyncModule()` to register `HttpClient`, `IMemoryCache`, `IAniListClient`, and the query handler

## 5. API Endpoint

- [x] 5.1 Create `AniListSyncEndpoints` class in `BloomWatch.Api` with `MapAniListSyncEndpoints()` extension method
- [x] 5.2 Implement `GET /api/anilist/search?query=...` endpoint with `[Authorize]`, query parameter validation, and proper error responses (400/401/502)
- [x] 5.3 Register `app.MapAniListSyncEndpoints()` in `Program.cs`

## 6. Testing

- [x] 6.1 Write unit tests for `SearchAnimeQueryHandler` (validation, delegation to `IAniListClient`)
- [x] 6.2 Write unit tests for the caching layer (cache hit, cache miss, case normalization)
- [x] 6.3 Write integration tests for `GET /api/anilist/search` endpoint (auth required, query validation, successful response shape, 502 on upstream failure)
