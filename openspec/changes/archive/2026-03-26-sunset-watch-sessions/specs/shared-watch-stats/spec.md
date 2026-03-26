## REMOVED Requirements

### Requirement: Total watch sessions computation
**Reason**: Watch sessions feature is being sunset. The `totalWatchSessions` field has no data source.
**Migration**: Clients should remove any display of `totalWatchSessions` from shared stats rendering.

### Requirement: Most recent session date
**Reason**: Watch sessions feature is being sunset. The `mostRecentSessionDate` field has no data source.
**Migration**: Clients should remove any display of `mostRecentSessionDate` from shared stats rendering.

## MODIFIED Requirements

### Requirement: Shared watch stats endpoint
The system SHALL expose `GET /watchspaces/{id}/analytics/shared-stats` as an authenticated, member-only endpoint that returns aggregate statistics about the watch space's shared anime tracking history.

#### Scenario: Successful stats retrieval with data
- **WHEN** an authenticated member of watch space `{id}` sends `GET /watchspaces/{id}/analytics/shared-stats` and the watch space contains anime
- **THEN** the system SHALL return `200 OK` with a JSON object containing `totalEpisodesWatchedTogether`, `totalFinished`, and `totalDropped`

#### Scenario: Empty watch space
- **WHEN** an authenticated member sends `GET /watchspaces/{id}/analytics/shared-stats` and the watch space contains no anime
- **THEN** the system SHALL return `200 OK` with `totalEpisodesWatchedTogether` as 0, `totalFinished` as 0, and `totalDropped` as 0

### Requirement: Response shape
The endpoint SHALL return a JSON object with exactly three fields: `totalEpisodesWatchedTogether` (integer), `totalFinished` (integer), and `totalDropped` (integer).

#### Scenario: Response contains only the three stat fields
- **WHEN** the endpoint returns `200 OK`
- **THEN** the response SHALL contain `totalEpisodesWatchedTogether`, `totalFinished`, and `totalDropped` and SHALL NOT contain `totalWatchSessions` or `mostRecentSessionDate`
