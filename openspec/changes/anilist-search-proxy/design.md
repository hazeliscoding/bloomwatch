## Context

BloomWatch is a modular monolith (.NET 10, ASP.NET Core Minimal APIs, PostgreSQL) with two existing modules — Identity and WatchSpaces. Each module follows a four-layer structure: Domain, Application, Infrastructure, and Contracts.

Story 3.1 introduces the first external API integration. AniList provides a free, public GraphQL API at `https://graphql.anilist.co` that requires no API key for read-only queries. The search proxy endpoint will allow authenticated BloomWatch users to search for anime by name, with results mapped to internal DTOs and cached in memory.

## Goals / Non-Goals

**Goals:**
- Establish the `AniListSync` module following the same architecture as existing modules
- Provide a clean, typed `GET /api/anilist/search?query=...` endpoint returning AniList search results
- Cache identical search queries in memory (5-minute TTL) to reduce AniList traffic
- Handle AniList failures gracefully with 502 responses
- Lay groundwork for Story 3.2 (media detail endpoint) to reuse the same GraphQL client infrastructure

**Non-Goals:**
- Persistent database caching of media metadata (that's Story 3.2's concern with `anilist_sync.media_cache`)
- Rate limiting or request throttling against AniList (AniList's public API has generous limits; revisit if needed)
- Pagination of search results (AniList search returns a manageable page; pass through as-is)
- OAuth integration with AniList user accounts

## Decisions

### 1. New AniListSync module with standard four-layer structure

Create `src/Modules/AniListSync/` with Domain, Application, Infrastructure, and Contracts projects, matching the Identity and WatchSpaces pattern. Even though Story 3.1 is lightweight, the module will grow with Story 3.2+ and needs a consistent home.

**Alternative considered:** Placing AniList logic directly in the API project as a simple service class. Rejected because it breaks the modular monolith pattern and makes Story 3.2 harder to integrate cleanly.

### 2. Typed HttpClient for AniList GraphQL

Register a named/typed `HttpClient` via `IHttpClientFactory` in the Infrastructure layer. The client sends POST requests to AniList's GraphQL endpoint with the search query embedded in the request body.

**Alternative considered:** Using a GraphQL client library (e.g., `GraphQL.Client`). Rejected because the queries are few and static — a typed `HttpClient` with hand-crafted query strings is simpler and avoids an extra dependency.

### 3. IMemoryCache for short-lived response caching

Use `Microsoft.Extensions.Caching.Memory.IMemoryCache` with a 5-minute absolute expiration keyed by the normalized search query (lowercased, trimmed). This is lightweight, requires no external infrastructure, and matches the story's requirements.

**Alternative considered:** Distributed cache (Redis). Unnecessary for a single-instance MVP — adds operational complexity without meaningful benefit.

### 4. Application-layer abstraction for AniList client

Define an `IAniListClient` interface in the Application layer with a `SearchAnimeAsync(string query)` method. The Infrastructure layer provides the implementation using `HttpClient`. This keeps the Application layer testable without hitting AniList.

### 5. Query handler pattern consistent with existing modules

Use a `SearchAnimeQuery` / `SearchAnimeQueryHandler` pair in the Application layer, matching the command/query handler pattern used in Identity (e.g., `LoginUserCommand` / `LoginUserCommandHandler`).

## Risks / Trade-offs

**[AniList API availability]** → The endpoint depends on an external service. Mitigation: 502 error handling with descriptive messages; in-memory cache reduces exposure to transient failures.

**[AniList response schema changes]** → AniList could change their GraphQL schema. Mitigation: The GraphQL query requests specific fields; deserialization uses lenient mapping (missing fields become null). Breaking changes would surface as empty results, not crashes.

**[Memory cache unbounded growth]** → Unique queries could accumulate entries. Mitigation: 5-minute TTL naturally evicts entries; `MemoryCache` has a `SizeLimit` option if needed in the future.
