## ADDED Requirements

### Requirement: Record a watch session
The system SHALL expose `POST /watchspaces/{id}/anime/{watchSpaceAnimeId}/sessions` as an authenticated, member-only endpoint that accepts a JSON body with required fields `sessionDateUtc` (ISO 8601 timestamp), `startEpisode` (int), `endEpisode` (int), and optional field `notes` (string), and creates a new `WatchSession` record owned by the specified `WatchSpaceAnime` aggregate. The `createdByUserId` SHALL be set to the authenticated user's ID.

#### Scenario: Successful session creation
- **WHEN** an authenticated member of watch space `{id}` sends `POST /watchspaces/{id}/anime/{watchSpaceAnimeId}/sessions` with body `{ "sessionDateUtc": "2026-03-15T20:00:00Z", "startEpisode": 1, "endEpisode": 3, "notes": "Great first session!" }`
- **THEN** the system SHALL create a `WatchSession` with the provided values, set `createdByUserId` to the caller's user ID, and return `201 Created` with the session details including `id`, `sessionDateUtc`, `startEpisode`, `endEpisode`, `notes`, and `createdByUserId`

#### Scenario: Successful session creation without notes
- **WHEN** an authenticated member sends a POST with body `{ "sessionDateUtc": "2026-03-15T20:00:00Z", "startEpisode": 5, "endEpisode": 5 }`
- **THEN** the system SHALL create the session with `notes` as null and return `201 Created`

### Requirement: Episode range validation
The system SHALL validate that `startEpisode` is >= 1 and `endEpisode` is >= `startEpisode`.

#### Scenario: Start episode is zero
- **WHEN** a POST request includes `{ "sessionDateUtc": "2026-03-15T20:00:00Z", "startEpisode": 0, "endEpisode": 3 }`
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating start episode must be at least 1

#### Scenario: Start episode is negative
- **WHEN** a POST request includes `{ "sessionDateUtc": "2026-03-15T20:00:00Z", "startEpisode": -1, "endEpisode": 3 }`
- **THEN** the system SHALL return `400 Bad Request`

#### Scenario: End episode less than start episode
- **WHEN** a POST request includes `{ "sessionDateUtc": "2026-03-15T20:00:00Z", "startEpisode": 5, "endEpisode": 3 }`
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating end episode must be greater than or equal to start episode

#### Scenario: Single-episode session (start equals end)
- **WHEN** a POST request includes `{ "sessionDateUtc": "2026-03-15T20:00:00Z", "startEpisode": 7, "endEpisode": 7 }`
- **THEN** the system SHALL accept the values and create the session

### Requirement: Session date validation
The system SHALL validate that `sessionDateUtc` is a valid ISO 8601 timestamp. Future dates are allowed.

#### Scenario: Valid past date
- **WHEN** a POST request includes `{ "sessionDateUtc": "2026-03-10T18:00:00Z", "startEpisode": 1, "endEpisode": 2 }`
- **THEN** the system SHALL accept the date and create the session

#### Scenario: Future date is allowed
- **WHEN** a POST request includes `{ "sessionDateUtc": "2026-12-25T20:00:00Z", "startEpisode": 1, "endEpisode": 1 }`
- **THEN** the system SHALL accept the date and create the session

#### Scenario: Invalid date format
- **WHEN** a POST request includes `{ "sessionDateUtc": "not-a-date", "startEpisode": 1, "endEpisode": 1 }`
- **THEN** the system SHALL return `400 Bad Request`

### Requirement: Membership enforcement for session recording
The system SHALL verify that the requesting user is a member of the watch space before allowing session creation. Membership SHALL be checked before any other operation.

#### Scenario: Non-member attempt
- **WHEN** an authenticated user who is NOT a member of watch space `{id}` sends a POST to create a session
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated attempt
- **WHEN** an unauthenticated request sends a POST to create a session
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Not-found handling for session recording
The system SHALL return 404 when the specified anime does not exist in the watch space. Membership SHALL be checked before the existence check to prevent information leakage.

#### Scenario: Anime not found in watch space
- **WHEN** an authenticated member sends a POST to `/watchspaces/{id}/anime/{watchSpaceAnimeId}/sessions` and no anime with that ID exists in the watch space
- **THEN** the system SHALL return `404 Not Found`

#### Scenario: Valid anime ID but wrong watch space
- **WHEN** an authenticated member sends a POST where the anime ID belongs to a different watch space
- **THEN** the system SHALL return `404 Not Found`

### Requirement: Response shape for session creation
The system SHALL return the created watch session as a JSON object containing `id` (GUID), `sessionDateUtc` (ISO 8601 timestamp), `startEpisode` (int), `endEpisode` (int), `notes` (string or null), and `createdByUserId` (GUID).

#### Scenario: Response includes all session fields
- **WHEN** a POST returns `201 Created`
- **THEN** the response SHALL contain: `id` (as GUID), `sessionDateUtc` (as ISO 8601 timestamp), `startEpisode` (as int), `endEpisode` (as int), `notes` (string or null), and `createdByUserId` (as GUID)

#### Scenario: 201 Created includes Location header
- **WHEN** a POST successfully creates a session
- **THEN** the response SHALL include a `201 Created` status code
