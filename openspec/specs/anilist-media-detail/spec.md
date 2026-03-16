### Requirement: Authenticated media detail endpoint
The system SHALL expose `GET /api/anilist/media/{anilistMediaId}` as an authenticated endpoint that returns full cached metadata for a single AniList anime by its numeric ID.

#### Scenario: Successful detail fetch (cache miss)
- **WHEN** an authenticated user sends `GET /api/anilist/media/1` and no cached entry exists for media ID 1
- **THEN** the system fetches the metadata from AniList GraphQL, stores it in the `anilist_sync.media_cache` table, and returns 200 OK with a JSON object containing: `anilistMediaId`, `titleRomaji`, `titleEnglish`, `titleNative`, `coverImageUrl`, `episodes`, `status`, `format`, `season`, `seasonYear`, `genres`, `description`, `averageScore`, `popularity`, `cachedAt`

#### Scenario: Successful detail fetch (cache hit, fresh)
- **WHEN** an authenticated user sends `GET /api/anilist/media/1` and a cached entry for media ID 1 exists with `cachedAt` within the last 24 hours
- **THEN** the system returns 200 OK with the cached data without calling the AniList API

#### Scenario: Cache entry is stale
- **WHEN** an authenticated user sends `GET /api/anilist/media/1` and a cached entry exists but `cachedAt` is older than 24 hours
- **THEN** the system fetches fresh metadata from AniList GraphQL, updates the cache entry, and returns 200 OK with the refreshed data

#### Scenario: Unauthenticated request
- **WHEN** a request is sent to `GET /api/anilist/media/1` without a valid JWT
- **THEN** the system returns 401 Unauthorized

### Requirement: Not-found handling for unknown AniList media IDs
The system SHALL return 404 when the requested AniList media ID does not exist on AniList.

#### Scenario: AniList returns no data for the ID
- **WHEN** an authenticated user sends `GET /api/anilist/media/999999999` and AniList returns no media data for that ID
- **THEN** the system returns 404 Not Found

#### Scenario: Previously cached entry for a now-removed AniList ID
- **WHEN** a cached entry exists for media ID X but the cache is stale, and AniList now returns no data for ID X
- **THEN** the system returns 404 Not Found and the stale cache entry is not served

### Requirement: AniList upstream error handling for detail endpoint
The system SHALL handle AniList API failures gracefully, returning 502 Bad Gateway without corrupting existing cache entries.

#### Scenario: AniList returns HTTP error during cache miss
- **WHEN** the AniList GraphQL API returns an HTTP error while fetching a media ID with no cached entry
- **THEN** the system returns 502 Bad Gateway with a descriptive error message

#### Scenario: AniList returns HTTP error during cache refresh
- **WHEN** the AniList GraphQL API returns an HTTP error while refreshing a stale cached entry
- **THEN** the system returns 502 Bad Gateway and the existing stale cache entry is preserved (not deleted or modified)

#### Scenario: AniList is unreachable during detail fetch
- **WHEN** the AniList GraphQL API is unreachable (connection timeout or DNS failure) during a detail fetch
- **THEN** the system returns 502 Bad Gateway with a descriptive error message

#### Scenario: AniList returns malformed response for detail fetch
- **WHEN** the AniList GraphQL API returns a response that cannot be deserialized for a detail fetch
- **THEN** the system returns 502 Bad Gateway with a descriptive error message

### Requirement: Persistent database caching with 24-hour freshness
The system SHALL store fetched media metadata in the `anilist_sync.media_cache` database table with a `cachedAt` timestamp and treat entries older than 24 hours as stale.

#### Scenario: Cache entry is written on first fetch
- **WHEN** the system successfully fetches metadata from AniList for a media ID with no existing cache entry
- **THEN** a new row is inserted into `anilist_sync.media_cache` with all metadata fields and `cachedAt` set to the current UTC time

#### Scenario: Cache entry is updated on stale refresh
- **WHEN** the system successfully fetches fresh metadata from AniList for a media ID with a stale cache entry
- **THEN** the existing row in `anilist_sync.media_cache` is updated with the new metadata and `cachedAt` is reset to the current UTC time

#### Scenario: Concurrent requests for the same uncached ID
- **WHEN** two requests arrive simultaneously for the same uncached media ID
- **THEN** both requests succeed (no errors) and the cache entry reflects the data from one of the fetches (upsert semantics)

### Requirement: Detail response includes cachedAt timestamp
The system SHALL include the `cachedAt` UTC timestamp in the detail response so the client can determine data freshness.

#### Scenario: Response includes cachedAt field
- **WHEN** an authenticated user fetches media detail and receives a 200 OK response
- **THEN** the response JSON includes a `cachedAt` field with a UTC datetime indicating when the data was last fetched from AniList
