## Context

The anime detail page (`AnimeDetail` component) renders metadata from the `GET /watchspaces/{id}/anime/{animeId}` endpoint. The frontend template already has conditional blocks for `genres`, `description`, `anilistScore`, and `anilistPopularity`, but these fields are never populated because the backend `GetWatchSpaceAnimeDetailQueryHandler` reads only from the `IAnimeTrackingRepository` (the `WatchSpaceAnime` aggregate), which stores only snapshot fields captured at add time (title, cover, episodes, format, season, year).

The richer metadata (genres, description, score, popularity) already lives in `anilist_sync.media_cache` via `MediaCacheEntry`, but the AnimeTracking module's detail handler does not perform a cross-module read to enrich its response.

AniList tags and site URL are not fetched or cached at all today.

## Goals / Non-Goals

**Goals:**
- Surface genres, description, AniList score, and popularity on the anime detail page (data already cached, just not wired)
- Fetch and cache AniList tags (name + rank + spoiler flag) and site URL from the AniList GraphQL API
- Display tags on the anime detail page with spoiler-tag hiding
- Display a clickable external AniList link on the anime detail page
- Display the anime's airing status (e.g., "Releasing", "Finished") as a badge
- Render the description as sanitized HTML instead of plain text (AniList returns HTML)

**Non-Goals:**
- Fetching or displaying studio information (would require a separate GraphQL fragment and entity changes disproportionate to the value)
- Making the frontend call the AniList media detail endpoint separately (keep the single API call pattern)
- Changing the anime list page -- this is detail-page-only
- Adding tag-based filtering or search

## Decisions

### 1. Cross-module enrichment via adapter interface

The `GetWatchSpaceAnimeDetailQueryHandler` will call a new `IMediaCacheReader` adapter (defined in AnimeTracking.Application, implemented in AnimeTracking.Infrastructure) to look up cached AniList data by `anilistMediaId`. This follows the existing cross-module adapter pattern used for membership checking (`IMembershipChecker`).

**Why not a direct EF Core query?** The AnimeTracking module should not reference AniListSync's DbContext directly. The adapter abstracts the cross-module boundary and can be backed by a simple read query.

**Why not a separate frontend API call?** The frontend currently makes one call for the detail. Adding a second call to `/api/anilist/media/{id}` would introduce loading state complexity and a waterfall request. Enriching server-side keeps the frontend simple.

### 2. Tags stored as JSONB on MediaCacheEntry

Tags will be stored as a `IReadOnlyList<MediaTag>` on `MediaCacheEntry`, persisted as a JSONB column (matching the existing `Genres` column pattern). Each `MediaTag` has `Name` (string), `Rank` (int), and `IsMediaSpoiler` (bool).

**Why not a separate tags table?** Tags are fetched and replaced wholesale on cache refresh. A JSONB column avoids join complexity and matches how genres are already stored.

### 3. Site URL stored as a nullable string on MediaCacheEntry

The AniList `siteUrl` field (e.g., `https://anilist.co/anime/154587`) will be stored as a simple `string?` column. It could be constructed from the media ID, but storing it from the API ensures correctness if AniList ever changes URL patterns.

### 4. HTML description rendering with Angular `[innerHTML]` + DOMPurify

AniList descriptions contain HTML (primarily `<br>`, `<i>`, `<b>` tags). The current template renders via `{{ }}` interpolation which escapes HTML. The change will use Angular's `[innerHTML]` binding with a sanitization pipe backed by DOMPurify (or Angular's built-in DomSanitizer) to allow safe HTML tags while stripping scripts.

**Why DomSanitizer (built-in) over DOMPurify?** Angular's `DomSanitizer.bypassSecurityTrustHtml()` is available without adding a dependency. Since AniList descriptions are a trusted data source with limited HTML tags, this is sufficient. A custom pipe will encapsulate the sanitization.

### 5. Spoiler tags hidden by default with reveal toggle

Tags with `isMediaSpoiler: true` will be visually obscured (blurred text, "Spoiler" label) with a click-to-reveal interaction. This respects AniList's spoiler metadata and prevents accidental plot reveals.

### 6. Extended `GetWatchSpaceAnimeDetailResult` DTO

The result record gains new optional fields: `Genres`, `Description`, `AverageScore`, `Popularity`, `Tags`, `SiteUrl`, `AiringStatus`. These are nullable to handle the case where the media cache entry doesn't exist or fields are missing.

## Risks / Trade-offs

**[Risk] Media cache miss for the enrichment read** -- If an anime was added to a watch space but the media cache entry was evicted or corrupted, the enrichment fields will be null. The frontend already handles absent optional fields gracefully.
-> Mitigation: The cache entry should always exist since adding anime requires a prior cache population. The adapter returns null fields rather than throwing.

**[Risk] Migration on production database** -- Adding JSONB and text columns to an existing table requires an EF Core migration.
-> Mitigation: New columns are nullable with no default, so the migration is additive and non-breaking. Existing rows will have null tags/siteUrl until their cache entry is refreshed.

**[Risk] AniList rate limiting when fetching new fields** -- The expanded GraphQL query fetches more data per request but does not increase request frequency.
-> Mitigation: No impact -- same number of API calls, slightly larger response payload.

**[Trade-off] Stale enrichment data** -- The enrichment data comes from the 24-hour cache, so it can be up to 24 hours behind AniList. This is acceptable for metadata that changes infrequently (genres, tags, description).
