### Requirement: Shared watch stats endpoint
The system SHALL expose `GET /watchspaces/{id}/analytics/shared-stats` as an authenticated, member-only endpoint that returns aggregate statistics about the watch space's shared watch history.

#### Scenario: Successful stats retrieval with data
- **WHEN** an authenticated member of watch space `{id}` sends `GET /watchspaces/{id}/analytics/shared-stats` and the watch space contains anime and watch sessions
- **THEN** the system SHALL return `200 OK` with a JSON object containing `totalEpisodesWatchedTogether`, `totalFinished`, `totalDropped`, `totalWatchSessions`, and `mostRecentSessionDate`

#### Scenario: Empty watch space
- **WHEN** an authenticated member sends `GET /watchspaces/{id}/analytics/shared-stats` and the watch space contains no anime and no watch sessions
- **THEN** the system SHALL return `200 OK` with `totalEpisodesWatchedTogether` as 0, `totalFinished` as 0, `totalDropped` as 0, `totalWatchSessions` as 0, and `mostRecentSessionDate` as `null`

### Requirement: Total episodes watched together computation
The `totalEpisodesWatchedTogether` field SHALL be the sum of `sharedEpisodesWatched` across all anime in the watch space, regardless of shared status.

#### Scenario: Multiple anime with episodes
- **WHEN** a watch space has anime A with `sharedEpisodesWatched = 12`, anime B with `sharedEpisodesWatched = 5`, and anime C with `sharedEpisodesWatched = 0`
- **THEN** `totalEpisodesWatchedTogether` SHALL be 17

#### Scenario: No anime in watch space
- **WHEN** a watch space has no anime
- **THEN** `totalEpisodesWatchedTogether` SHALL be 0

### Requirement: Total finished computation
The `totalFinished` field SHALL be the count of anime in the watch space with `sharedStatus = Finished`.

#### Scenario: Some anime finished
- **WHEN** a watch space has 2 anime with `sharedStatus = Finished`, 1 with `Watching`, and 1 with `Backlog`
- **THEN** `totalFinished` SHALL be 2

#### Scenario: No finished anime
- **WHEN** a watch space has no anime with `sharedStatus = Finished`
- **THEN** `totalFinished` SHALL be 0

### Requirement: Total dropped computation
The `totalDropped` field SHALL be the count of anime in the watch space with `sharedStatus = Dropped`.

#### Scenario: Some anime dropped
- **WHEN** a watch space has 1 anime with `sharedStatus = Dropped` and 3 with other statuses
- **THEN** `totalDropped` SHALL be 1

#### Scenario: No dropped anime
- **WHEN** a watch space has no anime with `sharedStatus = Dropped`
- **THEN** `totalDropped` SHALL be 0

### Requirement: Total watch sessions computation
The `totalWatchSessions` field SHALL be the count of all watch session records associated with any anime in the watch space.

#### Scenario: Multiple sessions across anime
- **WHEN** anime A has 3 watch sessions and anime B has 2 watch sessions in the watch space
- **THEN** `totalWatchSessions` SHALL be 5

#### Scenario: No watch sessions
- **WHEN** the watch space has no recorded watch sessions
- **THEN** `totalWatchSessions` SHALL be 0

### Requirement: Most recent session date
The `mostRecentSessionDate` field SHALL be the `sessionDateUtc` of the most recent watch session across all anime in the watch space, or `null` if no watch sessions exist.

#### Scenario: Multiple sessions with different dates
- **WHEN** the watch space has sessions with dates 2026-01-10, 2026-02-15, and 2026-01-25
- **THEN** `mostRecentSessionDate` SHALL be 2026-02-15

#### Scenario: No sessions exist
- **WHEN** the watch space has no watch sessions
- **THEN** `mostRecentSessionDate` SHALL be `null`

### Requirement: Membership enforcement
The system SHALL verify that the requesting user is a member of the watch space before returning shared stats data. Non-members SHALL receive `403 Forbidden`.

#### Scenario: Non-member requests shared stats
- **WHEN** an authenticated user who is NOT a member of watch space `{id}` sends `GET /watchspaces/{id}/analytics/shared-stats`
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated request sends `GET /watchspaces/{id}/analytics/shared-stats`
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Response shape
The endpoint SHALL return a JSON object with exactly five fields: `totalEpisodesWatchedTogether` (integer), `totalFinished` (integer), `totalDropped` (integer), `totalWatchSessions` (integer), and `mostRecentSessionDate` (ISO 8601 datetime string or `null`).

#### Scenario: Full response shape
- **WHEN** the endpoint returns successfully
- **THEN** the response SHALL be a JSON object with `totalEpisodesWatchedTogether` as an integer, `totalFinished` as an integer, `totalDropped` as an integer, `totalWatchSessions` as an integer, and `mostRecentSessionDate` as an ISO 8601 string or `null`
