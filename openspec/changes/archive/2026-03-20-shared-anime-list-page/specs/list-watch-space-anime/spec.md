## MODIFIED Requirements

### Requirement: List anime in a watch space

The system SHALL provide a `GET /watchspaces/{watchSpaceId}/anime` endpoint that returns all anime tracked in the specified watch space. The response SHALL include each anime's shared tracking state and a summary of participant entries. The endpoint SHALL require authentication and membership in the watch space.

#### Scenario: Successful listing with tracked anime

- **WHEN** an authenticated member of watch space `{watchSpaceId}` sends `GET /watchspaces/{watchSpaceId}/anime`
- **THEN** the system SHALL return `200 OK` with a JSON array of anime items
- **AND** each item SHALL include: `watchSpaceAnimeId`, `anilistMediaId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `sharedStatus`, `sharedEpisodesWatched`, `addedAtUtc`
- **AND** each item SHALL include a `participants` array where each entry contains: `userId`, `displayName`, `individualStatus`, `episodesWatched`
- **AND** the frontend `WatchSpaceAnimeListItem` model SHALL include a `participants` array field matching this response shape

#### Scenario: Empty watch space

- **WHEN** an authenticated member sends `GET /watchspaces/{watchSpaceId}/anime` and no anime have been added to the watch space
- **THEN** the system SHALL return `200 OK` with an empty JSON array

#### Scenario: Non-member access

- **WHEN** an authenticated user who is NOT a member of watch space `{watchSpaceId}` sends `GET /watchspaces/{watchSpaceId}/anime`
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated access

- **WHEN** an unauthenticated request is sent to `GET /watchspaces/{watchSpaceId}/anime`
- **THEN** the system SHALL return `401 Unauthorized`
