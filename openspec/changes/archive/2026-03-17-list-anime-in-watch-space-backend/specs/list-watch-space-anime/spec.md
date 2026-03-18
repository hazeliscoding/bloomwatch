## ADDED Requirements

### Requirement: List anime in a watch space

The system SHALL provide a `GET /watchspaces/{watchSpaceId}/anime` endpoint that returns all anime tracked in the specified watch space. The response SHALL include each anime's shared tracking state and a summary of participant entries. The endpoint SHALL require authentication and membership in the watch space.

#### Scenario: Successful listing with tracked anime

- **WHEN** an authenticated member of watch space `{watchSpaceId}` sends `GET /watchspaces/{watchSpaceId}/anime`
- **THEN** the system SHALL return `200 OK` with a JSON array of anime items
- **AND** each item SHALL include: `watchSpaceAnimeId`, `anilistMediaId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `sharedStatus`, `sharedEpisodesWatched`, `addedAtUtc`
- **AND** each item SHALL include a `participants` array where each entry contains: `userId`, `individualStatus`, `episodesWatched`

#### Scenario: Empty watch space

- **WHEN** an authenticated member sends `GET /watchspaces/{watchSpaceId}/anime` and no anime have been added to the watch space
- **THEN** the system SHALL return `200 OK` with an empty JSON array

#### Scenario: Non-member access

- **WHEN** an authenticated user who is NOT a member of watch space `{watchSpaceId}` sends `GET /watchspaces/{watchSpaceId}/anime`
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated access

- **WHEN** an unauthenticated request is sent to `GET /watchspaces/{watchSpaceId}/anime`
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Filter anime by shared status

The endpoint SHALL support an optional `status` query parameter that filters the returned anime by their shared status value.

#### Scenario: Filter by a valid status

- **WHEN** a member sends `GET /watchspaces/{watchSpaceId}/anime?status=watching`
- **THEN** the system SHALL return only anime where `sharedStatus` equals `watching`
- **AND** the response SHALL be `200 OK`

#### Scenario: Filter with no matches

- **WHEN** a member sends `GET /watchspaces/{watchSpaceId}/anime?status=finished` and no anime have the `finished` shared status
- **THEN** the system SHALL return `200 OK` with an empty JSON array

#### Scenario: No status filter provided

- **WHEN** a member sends `GET /watchspaces/{watchSpaceId}/anime` without a `status` query parameter
- **THEN** the system SHALL return all anime in the watch space regardless of shared status

### Requirement: Consistent result ordering

The system SHALL return anime in a consistent, deterministic order.

#### Scenario: Default ordering

- **WHEN** a member retrieves the anime list
- **THEN** results SHALL be ordered by `addedAtUtc` descending (most recently added first)
