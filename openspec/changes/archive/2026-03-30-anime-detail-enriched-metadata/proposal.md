## Why

The anime detail page currently shows basic metadata (title, format, season, genres, description, AniList score) but is missing AniList tags, an external AniList link, the airing status, and proper HTML rendering of descriptions. Tags are one of AniList's most useful discovery features (e.g., "Isekai", "Coming of Age", "Time Skip") and are absent from both the backend cache and the frontend. Adding these enriches the detail page into a genuinely useful reference panel and reduces the need to leave BloomWatch to check AniList directly.

## What Changes

- Extend the AniList GraphQL query to fetch `tags { name, rank, isMediaSpoiler }` and `siteUrl`
- Extend the `MediaCacheEntry` domain entity and database schema to persist tags and site URL
- Extend the anime detail backend response (`GetWatchSpaceAnimeDetailResult`) to surface tags, site URL, airing status, and description from the media cache
- Update the frontend `WatchSpaceAnimeDetail` TypeScript model to include new fields
- Enhance the anime detail page hero section to display: tags (with spoiler tag handling), a clickable AniList external link, the anime's airing status, and HTML-safe description rendering
- Add an EF Core migration for the new `MediaCacheEntry` columns

## Capabilities

### New Capabilities

- `anime-detail-enriched-metadata`: Covers the full vertical slice of fetching, caching, surfacing, and displaying AniList tags, site URL, and airing status on the anime detail page, plus improving description rendering from plain text to sanitized HTML.

### Modified Capabilities

- `anilist-media-detail`: The GraphQL query and cache entity gain new fields (tags, siteUrl). The media detail response shape expands.
- `anime-detail-view`: The hero section gains new UI elements (tags, external link, airing status badge, HTML description).

## Impact

- **Backend / AniListSync module**: `AniListGraphQlClient` GraphQL query, `MediaCacheEntry` entity, `AnimeMediaDetail` DTO, EF Core configuration and migration
- **Backend / AnimeTracking module**: `GetWatchSpaceAnimeDetailResult` response DTO, query handler (cross-module media cache read)
- **Frontend / watch-spaces feature**: `WatchSpaceAnimeDetail` TypeScript interface, `AnimeDetail` component template and SCSS
- **Database**: New JSONB column for tags and text column for site URL on `anilist_sync.media_cache`
- **Existing tests**: Backend unit/integration tests for AniList media detail and anime tracking detail endpoints need updates for new fields
