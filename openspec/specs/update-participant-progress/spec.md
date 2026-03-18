### Requirement: Update individual participant progress
The system SHALL expose `PATCH /watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-progress` as an authenticated, member-only endpoint that accepts a JSON body with required fields `episodesWatched` (int) and `individualStatus` (enum string), and applies them to the requesting user's own `ParticipantEntry`. If no entry exists for the caller, the system SHALL create one (upsert semantics).

#### Scenario: Successful update of existing participant entry
- **WHEN** an authenticated member of watch space `{id}` sends `PATCH /watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-progress` with body `{ "episodesWatched": 7, "individualStatus": "Watching" }` and the member already has a `ParticipantEntry` for that anime
- **THEN** the system SHALL update `episodesWatched` to 7 and `individualStatus` to `Watching` on the existing entry, update `lastUpdatedAtUtc`, and return `200 OK` with the updated `ParticipantDetail`

#### Scenario: Upsert — create new participant entry
- **WHEN** an authenticated member sends a PATCH and no `ParticipantEntry` exists for that user on the specified anime
- **THEN** the system SHALL create a new `ParticipantEntry` with the provided `episodesWatched` and `individualStatus`, set `lastUpdatedAtUtc` to the current UTC time, and return `200 OK` with the new `ParticipantDetail`

#### Scenario: Update only applies to the caller's own entry
- **WHEN** an authenticated member sends a PATCH to update participant progress
- **THEN** the system SHALL modify only the `ParticipantEntry` belonging to the requesting user, leaving all other participants' entries unchanged

### Requirement: Individual status validation
The system SHALL validate that `individualStatus` is one of the allowed values: `Backlog`, `Watching`, `Finished`, `Paused`, `Dropped`.

#### Scenario: Invalid individual status value
- **WHEN** a PATCH request includes `{ "individualStatus": "Unknown", "episodesWatched": 0 }`
- **THEN** the system SHALL return `400 Bad Request`

#### Scenario: Valid individual status value
- **WHEN** a PATCH request includes `{ "individualStatus": "Dropped", "episodesWatched": 3 }`
- **THEN** the system SHALL accept the value and update the individual status

### Requirement: Episode count validation for participant progress
The system SHALL validate that `episodesWatched` is >= 0 and, when the anime's `episodeCountSnapshot` is known (non-null), <= `episodeCountSnapshot`.

#### Scenario: Negative episode count
- **WHEN** a PATCH request includes `{ "episodesWatched": -1, "individualStatus": "Watching" }`
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating the episode count must be non-negative

#### Scenario: Episode count exceeds known total
- **WHEN** a PATCH request includes `{ "episodesWatched": 25, "individualStatus": "Watching" }` and the anime's `episodeCountSnapshot` is 24
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating the episode count cannot exceed the total episode count

#### Scenario: Episode count at boundary (equal to total)
- **WHEN** a PATCH request includes `{ "episodesWatched": 24, "individualStatus": "Finished" }` and the anime's `episodeCountSnapshot` is 24
- **THEN** the system SHALL accept the value and update `episodesWatched` to 24

#### Scenario: Episode count when total is unknown
- **WHEN** a PATCH request includes `{ "episodesWatched": 100, "individualStatus": "Watching" }` and the anime's `episodeCountSnapshot` is null
- **THEN** the system SHALL accept the value (no upper-bound check)

### Requirement: Membership enforcement for participant progress update
The system SHALL verify that the requesting user is a member of the watch space before allowing the update. Membership SHALL be checked before any other operation.

#### Scenario: Non-member update attempt
- **WHEN** an authenticated user who is NOT a member of watch space `{id}` sends a PATCH to update participant progress
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated update attempt
- **WHEN** an unauthenticated request sends a PATCH to update participant progress
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Not-found handling for participant progress update
The system SHALL return 404 when the specified anime does not exist in the watch space. Membership SHALL be checked before the existence check to prevent information leakage.

#### Scenario: Anime not found in watch space
- **WHEN** an authenticated member sends a PATCH to `/watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-progress` and no anime with that ID exists in the watch space
- **THEN** the system SHALL return `404 Not Found`

#### Scenario: Valid anime ID but wrong watch space
- **WHEN** an authenticated member sends a PATCH where the anime ID belongs to a different watch space
- **THEN** the system SHALL return `404 Not Found`

### Requirement: Response shape for participant progress update
The system SHALL return the updated participant entry as a `ParticipantDetail` containing `userId`, `individualStatus`, `episodesWatched`, `ratingScore`, `ratingNotes`, and `lastUpdatedAtUtc`.

#### Scenario: Response includes all participant detail fields
- **WHEN** a PATCH returns `200 OK`
- **THEN** the response SHALL contain: `userId`, `individualStatus` (as string), `episodesWatched` (as int), `ratingScore` (decimal or null), `ratingNotes` (string or null), and `lastUpdatedAtUtc` (as ISO 8601 timestamp)

#### Scenario: Newly created entry has null rating fields
- **WHEN** a PATCH creates a new `ParticipantEntry` via upsert and returns `200 OK`
- **THEN** the response SHALL have `ratingScore` as null and `ratingNotes` as null
