## ADDED Requirements

### Requirement: Authenticated anime search endpoint
The system SHALL expose `GET /api/anilist/search?query={searchTerm}` as an authenticated endpoint that proxies search queries to the AniList GraphQL API and returns matching anime results.

#### Scenario: Successful search with results
- **WHEN** an authenticated user sends `GET /api/anilist/search?query=Cowboy Bebop`
- **THEN** the system returns 200 OK with a JSON array of matching anime, each containing: `anilistMediaId`, `titleRomaji`, `titleEnglish`, `coverImageUrl`, `episodes`, `status`, `format`, `season`, `seasonYear`, `genres`

#### Scenario: Successful search with no results
- **WHEN** an authenticated user sends `GET /api/anilist/search?query=xyznonexistent999`
- **THEN** the system returns 200 OK with an empty JSON array

#### Scenario: Unauthenticated request
- **WHEN** a request is sent to `GET /api/anilist/search?query=Naruto` without a valid JWT
- **THEN** the system returns 401 Unauthorized

### Requirement: Query parameter validation
The system SHALL require a non-empty `query` parameter and reject invalid requests.

#### Scenario: Missing query parameter
- **WHEN** an authenticated user sends `GET /api/anilist/search` with no `query` parameter
- **THEN** the system returns 400 Bad Request with a descriptive error message

#### Scenario: Blank query parameter
- **WHEN** an authenticated user sends `GET /api/anilist/search?query=` (empty string)
- **THEN** the system returns 400 Bad Request with a descriptive error message

#### Scenario: Whitespace-only query parameter
- **WHEN** an authenticated user sends `GET /api/anilist/search?query=%20%20`
- **THEN** the system returns 400 Bad Request with a descriptive error message

### Requirement: AniList relevance ordering
The system SHALL return search results in the order provided by AniList's relevance ranking without re-sorting.

#### Scenario: Results preserve AniList ordering
- **WHEN** an authenticated user searches for a term that returns multiple results
- **THEN** the results are ordered by AniList's default relevance ranking (most relevant first)

### Requirement: In-memory response caching
The system SHALL cache search responses in memory with a 5-minute TTL, keyed by normalized query string, to avoid redundant AniList API calls.

#### Scenario: Cache hit for identical query
- **WHEN** an authenticated user searches for "Attack on Titan" and the same query was made within the last 5 minutes
- **THEN** the system returns the cached response without calling the AniList API

#### Scenario: Cache hit for case-variant query
- **WHEN** a user searches for "attack on titan" after "Attack on Titan" was cached
- **THEN** the system returns the cached response (query normalization is case-insensitive)

#### Scenario: Cache expiry
- **WHEN** more than 5 minutes have passed since a query was cached
- **THEN** the next request for that query fetches fresh results from AniList

### Requirement: AniList upstream error handling
The system SHALL handle AniList API failures gracefully, returning 502 Bad Gateway to the client without crashing the endpoint.

#### Scenario: AniList returns HTTP error
- **WHEN** the AniList GraphQL API returns an HTTP error (e.g., 500 Internal Server Error)
- **THEN** the system returns 502 Bad Gateway with a descriptive error message indicating the upstream failure

#### Scenario: AniList is unreachable
- **WHEN** the AniList GraphQL API is unreachable (connection timeout or DNS failure)
- **THEN** the system returns 502 Bad Gateway with a descriptive error message

#### Scenario: AniList returns malformed response
- **WHEN** the AniList GraphQL API returns a response that cannot be deserialized
- **THEN** the system returns 502 Bad Gateway with a descriptive error message
