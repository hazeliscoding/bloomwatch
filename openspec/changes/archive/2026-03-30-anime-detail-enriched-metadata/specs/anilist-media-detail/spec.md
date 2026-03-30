## MODIFIED Requirements

### Requirement: Authenticated media detail endpoint
The system SHALL expose `GET /api/anilist/media/{anilistMediaId}` as an authenticated endpoint that returns full cached metadata for a single AniList anime by its numeric ID.

#### Scenario: Successful detail fetch (cache miss)
- **WHEN** an authenticated user sends `GET /api/anilist/media/1` and no cached entry exists for media ID 1
- **THEN** the system fetches the metadata from AniList GraphQL, stores it in the `anilist_sync.media_cache` table, and returns 200 OK with a JSON object containing: `anilistMediaId`, `titleRomaji`, `titleEnglish`, `titleNative`, `coverImageUrl`, `episodes`, `status`, `format`, `season`, `seasonYear`, `genres`, `description`, `averageScore`, `popularity`, `tags` (array of objects with `name`, `rank`, `isMediaSpoiler`), `siteUrl`, `cachedAt`

#### Scenario: Successful detail fetch (cache hit, fresh)
- **WHEN** an authenticated user sends `GET /api/anilist/media/1` and a cached entry for media ID 1 exists with `cachedAt` within the last 24 hours
- **THEN** the system returns 200 OK with the cached data including `tags` and `siteUrl` without calling the AniList API

#### Scenario: Cache entry is stale
- **WHEN** an authenticated user sends `GET /api/anilist/media/1` and a cached entry exists but `cachedAt` is older than 24 hours
- **THEN** the system fetches fresh metadata from AniList GraphQL, updates the cache entry including `tags` and `siteUrl`, and returns 200 OK with the refreshed data

#### Scenario: Unauthenticated request
- **WHEN** a request is sent to `GET /api/anilist/media/1` without a valid JWT
- **THEN** the system returns 401 Unauthorized

## ADDED Requirements

### Requirement: AniList GraphQL query fetches tags and site URL
The AniList GraphQL media-by-ID query SHALL request `tags { name rank isMediaSpoiler }` and `siteUrl` fields in addition to all currently fetched fields.

#### Scenario: Tags included in GraphQL response
- **WHEN** the system fetches media metadata from AniList for a media ID
- **THEN** the GraphQL query SHALL include `tags { name rank isMediaSpoiler }` and the response SHALL be mapped to a list of tag objects

#### Scenario: Site URL included in GraphQL response
- **WHEN** the system fetches media metadata from AniList for a media ID
- **THEN** the GraphQL query SHALL include `siteUrl` and the response SHALL be mapped to a string field

### Requirement: MediaCacheEntry stores tags and site URL
The `MediaCacheEntry` entity SHALL include properties for AniList tags and site URL, persisted in the `anilist_sync.media_cache` table.

#### Scenario: Tags stored as JSONB
- **WHEN** a media cache entry is created or updated
- **THEN** the `Tags` property (a list of objects with `Name`, `Rank`, `IsMediaSpoiler`) SHALL be persisted as a JSONB column in the database

#### Scenario: Site URL stored as text
- **WHEN** a media cache entry is created or updated
- **THEN** the `SiteUrl` property (nullable string) SHALL be persisted as a text column in the database

#### Scenario: Existing cache entries have null tags and site URL
- **WHEN** the database migration runs against an existing database with media cache rows
- **THEN** the new `Tags` and `SiteUrl` columns SHALL default to null for existing rows

### Requirement: AnimeMediaDetail DTO includes tags and site URL
The `AnimeMediaDetail` application-layer DTO SHALL include `Tags` (list of tag objects) and `SiteUrl` (string) fields to carry the data from the AniList client through to the cache.

#### Scenario: DTO carries tag data
- **WHEN** the AniList GraphQL client returns media data
- **THEN** the `AnimeMediaDetail` record SHALL include `Tags` as `IReadOnlyList<AnimeMediaTag>` where each tag has `Name` (string), `Rank` (int), and `IsMediaSpoiler` (bool)

#### Scenario: DTO carries site URL
- **WHEN** the AniList GraphQL client returns media data
- **THEN** the `AnimeMediaDetail` record SHALL include `SiteUrl` as `string?`
