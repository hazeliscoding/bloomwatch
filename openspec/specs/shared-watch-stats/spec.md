### Requirement: Shared watch stats endpoint
The system SHALL expose `GET /watchspaces/{id}/analytics/shared-stats` as an authenticated, member-only endpoint that returns aggregate statistics about the watch space's shared anime tracking history.

#### Scenario: Successful stats retrieval with data
- **WHEN** an authenticated member of watch space `{id}` sends `GET /watchspaces/{id}/analytics/shared-stats` and the watch space contains anime
- **THEN** the system SHALL return `200 OK` with a JSON object containing `totalEpisodesWatchedTogether`, `totalFinished`, and `totalDropped`

#### Scenario: Empty watch space
- **WHEN** an authenticated member sends `GET /watchspaces/{id}/analytics/shared-stats` and the watch space contains no anime
- **THEN** the system SHALL return `200 OK` with `totalEpisodesWatchedTogether` as 0, `totalFinished` as 0, and `totalDropped` as 0

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

### Requirement: Membership enforcement
The system SHALL verify that the requesting user is a member of the watch space before returning shared stats data. Non-members SHALL receive `403 Forbidden`.

#### Scenario: Non-member requests shared stats
- **WHEN** an authenticated user who is NOT a member of watch space `{id}` sends `GET /watchspaces/{id}/analytics/shared-stats`
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated request sends `GET /watchspaces/{id}/analytics/shared-stats`
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Response shape
The endpoint SHALL return a JSON object with exactly three fields: `totalEpisodesWatchedTogether` (integer), `totalFinished` (integer), and `totalDropped` (integer).

#### Scenario: Response contains only the three stat fields
- **WHEN** the endpoint returns `200 OK`
- **THEN** the response SHALL contain `totalEpisodesWatchedTogether`, `totalFinished`, and `totalDropped` and SHALL NOT contain `totalWatchSessions` or `mostRecentSessionDate`
