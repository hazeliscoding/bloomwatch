## Why

Epic 3 (AniList Discovery) is a prerequisite for Epic 4 (Anime Tracking) — users cannot add anime to a watch space without first being able to search for it. Story 3.1 introduces the backend search proxy that lets authenticated users query AniList's GraphQL API through our own REST endpoint, keeping the AniList integration server-side and cacheable.

## What Changes

- Add a new `AniListSync` module following the same modular monolith structure as Identity and WatchSpaces (Domain, Application, Infrastructure, Contracts layers)
- Implement `GET /api/anilist/search?query=...` as an authenticated endpoint that proxies search queries to the AniList GraphQL API
- Build a typed GraphQL client using `HttpClient` to query AniList's public API (`https://graphql.anilist.co`)
- Introduce in-memory response caching (5-minute TTL) to avoid redundant AniList calls for identical search queries
- Map AniList GraphQL responses to internal DTOs exposing: `anilistMediaId`, `titleRomaji`, `titleEnglish`, `coverImageUrl`, `episodes`, `status`, `format`, `season`, `seasonYear`, `genres`
- Surface AniList API failures as 502 Bad Gateway with descriptive error messages

## Capabilities

### New Capabilities
- `anilist-search`: Authenticated search proxy endpoint that queries AniList GraphQL, maps results to internal DTOs, caches responses in memory, and handles upstream errors gracefully

### Modified Capabilities
_(none)_

## Impact

- **New module:** `src/Modules/AniListSync/` with Domain, Application, Infrastructure, and Contracts projects
- **API layer:** New endpoint group registered in `Program.cs` via `app.MapAniListSyncEndpoints()`
- **Dependencies:** `HttpClient` configured for AniList GraphQL; `IMemoryCache` for short-lived response caching
- **Configuration:** AniList base URL may be configurable for testing
- **Existing code:** No modifications to Identity or WatchSpaces modules — this is a purely additive change
