### Requirement: Submit or update a personal rating
The system SHALL expose `PATCH /watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-rating` as an authenticated, member-only endpoint that accepts a JSON body with required field `ratingScore` (decimal) and optional field `ratingNotes` (string), and applies them to the requesting user's own `ParticipantEntry`. If no entry exists for the caller, the system SHALL create one (upsert semantics) with default progress values (`Backlog`, 0 episodes watched).

#### Scenario: Successful update of existing participant rating
- **WHEN** an authenticated member of watch space `{id}` sends `PATCH /watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-rating` with body `{ "ratingScore": 8.5 }` and the member already has a `ParticipantEntry` for that anime
- **THEN** the system SHALL update `ratingScore` to 8.5 on the existing entry, update `lastUpdatedAtUtc`, and return `200 OK` with the updated `ParticipantDetail`

#### Scenario: Submit rating with optional notes
- **WHEN** an authenticated member sends a PATCH with body `{ "ratingScore": 7.0, "ratingNotes": "Great animation but slow pacing" }`
- **THEN** the system SHALL update both `ratingScore` to 7.0 and `ratingNotes` to the provided text, and return `200 OK`

#### Scenario: Overwrite previous rating (upsert)
- **WHEN** an authenticated member who previously rated an anime 6.0 sends a PATCH with body `{ "ratingScore": 9.0 }`
- **THEN** the system SHALL overwrite the existing `ratingScore` with 9.0 and return `200 OK` with the updated entry

#### Scenario: Upsert — create new participant entry via rating
- **WHEN** an authenticated member sends a PATCH and no `ParticipantEntry` exists for that user on the specified anime
- **THEN** the system SHALL create a new `ParticipantEntry` with `ratingScore` set to the provided value, `individualStatus` defaulting to `Backlog`, `episodesWatched` defaulting to 0, and return `200 OK`

#### Scenario: Update only applies to the caller's own entry
- **WHEN** an authenticated member sends a PATCH to update participant rating
- **THEN** the system SHALL modify only the `ParticipantEntry` belonging to the requesting user, leaving all other participants' entries unchanged

#### Scenario: Rating update does not affect progress fields
- **WHEN** an authenticated member with `episodesWatched: 12` and `individualStatus: Watching` sends a PATCH with body `{ "ratingScore": 8.0 }`
- **THEN** the system SHALL update only the rating fields; `episodesWatched` SHALL remain 12 and `individualStatus` SHALL remain `Watching`

### Requirement: Rating score validation
The system SHALL validate that `ratingScore` is between 0.5 and 10.0 inclusive, in 0.5 increments (i.e., valid values are 0.5, 1.0, 1.5, 2.0, ..., 9.5, 10.0).

#### Scenario: Rating below minimum
- **WHEN** a PATCH request includes `{ "ratingScore": 0.0 }`
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating the rating must be between 0.5 and 10.0

#### Scenario: Rating above maximum
- **WHEN** a PATCH request includes `{ "ratingScore": 10.5 }`
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating the rating must be between 0.5 and 10.0

#### Scenario: Rating not in 0.5 increments
- **WHEN** a PATCH request includes `{ "ratingScore": 7.3 }`
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating the rating must be in 0.5 increments

#### Scenario: Valid minimum rating
- **WHEN** a PATCH request includes `{ "ratingScore": 0.5 }`
- **THEN** the system SHALL accept the value and update the rating

#### Scenario: Valid maximum rating
- **WHEN** a PATCH request includes `{ "ratingScore": 10.0 }`
- **THEN** the system SHALL accept the value and update the rating

#### Scenario: Valid mid-range rating
- **WHEN** a PATCH request includes `{ "ratingScore": 7.5 }`
- **THEN** the system SHALL accept the value and update the rating

### Requirement: Rating notes validation
The system SHALL validate that `ratingNotes`, when provided, does not exceed 1000 characters.

#### Scenario: Notes within length limit
- **WHEN** a PATCH request includes `{ "ratingScore": 8.0, "ratingNotes": "Loved it" }`
- **THEN** the system SHALL accept the value and store the notes

#### Scenario: Notes exceeding length limit
- **WHEN** a PATCH request includes `{ "ratingScore": 8.0, "ratingNotes": "<1001+ characters>" }`
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating notes cannot exceed 1000 characters

#### Scenario: Notes omitted
- **WHEN** a PATCH request includes `{ "ratingScore": 8.0 }` with no `ratingNotes` field
- **THEN** the system SHALL accept the request; if the entry previously had notes, they SHALL remain unchanged

#### Scenario: Notes explicitly set to null
- **WHEN** a PATCH request includes `{ "ratingScore": 8.0, "ratingNotes": null }`
- **THEN** the system SHALL clear the existing notes, setting `ratingNotes` to null

### Requirement: Membership enforcement for participant rating update
The system SHALL verify that the requesting user is a member of the watch space before allowing the rating update. Membership SHALL be checked before any other operation.

#### Scenario: Non-member rating attempt
- **WHEN** an authenticated user who is NOT a member of watch space `{id}` sends a PATCH to update participant rating
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated rating attempt
- **WHEN** an unauthenticated request sends a PATCH to update participant rating
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Not-found handling for participant rating update
The system SHALL return 404 when the specified anime does not exist in the watch space. Membership SHALL be checked before the existence check to prevent information leakage.

#### Scenario: Anime not found in watch space
- **WHEN** an authenticated member sends a PATCH to `/watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-rating` and no anime with that ID exists in the watch space
- **THEN** the system SHALL return `404 Not Found`

#### Scenario: Valid anime ID but wrong watch space
- **WHEN** an authenticated member sends a PATCH where the anime ID belongs to a different watch space
- **THEN** the system SHALL return `404 Not Found`

### Requirement: Response shape for participant rating update
The system SHALL return the updated participant entry as a `ParticipantDetail` containing `userId`, `individualStatus`, `episodesWatched`, `ratingScore`, `ratingNotes`, and `lastUpdatedAtUtc`.

#### Scenario: Response includes all participant detail fields
- **WHEN** a PATCH returns `200 OK`
- **THEN** the response SHALL contain: `userId`, `individualStatus` (as string), `episodesWatched` (as int), `ratingScore` (as decimal), `ratingNotes` (as string or null), and `lastUpdatedAtUtc` (as ISO 8601 timestamp)

#### Scenario: Newly created entry has default progress fields
- **WHEN** a PATCH creates a new `ParticipantEntry` via upsert and returns `200 OK`
- **THEN** the response SHALL have `individualStatus` as `"Backlog"` and `episodesWatched` as 0
