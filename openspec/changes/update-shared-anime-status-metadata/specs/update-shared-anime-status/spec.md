## ADDED Requirements

### Requirement: Update shared anime status and metadata
The system SHALL expose `PATCH /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` as an authenticated, member-only endpoint that accepts a partial update body with optional fields `sharedStatus`, `sharedEpisodesWatched`, `mood`, `vibe`, and `pitch`, and applies only the provided fields to the shared tracking state of the specified anime.

#### Scenario: Successful full update
- **WHEN** an authenticated member of watch space `{watchSpaceId}` sends `PATCH /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` with body `{ "sharedStatus": "Watching", "sharedEpisodesWatched": 5, "mood": "hype", "vibe": "cozy nights", "pitch": "a must-watch classic" }`
- **THEN** the system SHALL update all five fields on the `WatchSpaceAnime` record and return `200 OK` with the full updated anime detail

#### Scenario: Partial update — only status
- **WHEN** an authenticated member sends a PATCH with body `{ "sharedStatus": "Finished" }` and no other fields
- **THEN** the system SHALL update only `sharedStatus` to `Finished`, leaving `sharedEpisodesWatched`, `mood`, `vibe`, and `pitch` unchanged, and return `200 OK`

#### Scenario: Partial update — only episodes watched
- **WHEN** an authenticated member sends a PATCH with body `{ "sharedEpisodesWatched": 12 }` and the anime's `episodeCountSnapshot` is 24
- **THEN** the system SHALL update only `sharedEpisodesWatched` to 12, leaving all other fields unchanged, and return `200 OK`

#### Scenario: Partial update — only metadata fields
- **WHEN** an authenticated member sends a PATCH with body `{ "mood": "nostalgic", "vibe": "rainy day vibes" }` and no other fields
- **THEN** the system SHALL update only `mood` and `vibe`, leaving `sharedStatus`, `sharedEpisodesWatched`, and `pitch` unchanged, and return `200 OK`

#### Scenario: Empty body (no fields provided)
- **WHEN** an authenticated member sends a PATCH with an empty body `{}`
- **THEN** the system SHALL return `200 OK` with the unchanged anime detail (no-op)

### Requirement: Shared status validation
The system SHALL validate that `sharedStatus`, when provided, is one of the allowed values: `Backlog`, `Watching`, `Finished`, `Paused`, `Dropped`.

#### Scenario: Invalid status value
- **WHEN** a PATCH request includes `{ "sharedStatus": "Unknown" }`
- **THEN** the system SHALL return `400 Bad Request`

#### Scenario: Valid status value
- **WHEN** a PATCH request includes `{ "sharedStatus": "Paused" }`
- **THEN** the system SHALL accept the value and update the shared status

### Requirement: Episode count validation
The system SHALL validate that `sharedEpisodesWatched`, when provided, is >= 0 and, when the anime's `episodeCountSnapshot` is known (non-null), <= `episodeCountSnapshot`.

#### Scenario: Negative episode count
- **WHEN** a PATCH request includes `{ "sharedEpisodesWatched": -1 }`
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating the episode count must be non-negative

#### Scenario: Episode count exceeds known total
- **WHEN** a PATCH request includes `{ "sharedEpisodesWatched": 25 }` and the anime's `episodeCountSnapshot` is 24
- **THEN** the system SHALL return `400 Bad Request` with an error message indicating the episode count cannot exceed the total episode count

#### Scenario: Episode count at boundary (equal to total)
- **WHEN** a PATCH request includes `{ "sharedEpisodesWatched": 24 }` and the anime's `episodeCountSnapshot` is 24
- **THEN** the system SHALL accept the value and update `sharedEpisodesWatched` to 24

#### Scenario: Episode count when total is unknown
- **WHEN** a PATCH request includes `{ "sharedEpisodesWatched": 100 }` and the anime's `episodeCountSnapshot` is null
- **THEN** the system SHALL accept the value (no upper-bound check)

### Requirement: Membership enforcement for update
The system SHALL verify that the requesting user is a member of the watch space before allowing any update.

#### Scenario: Non-member update attempt
- **WHEN** an authenticated user who is NOT a member of watch space `{watchSpaceId}` sends a PATCH to update an anime in that watch space
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated update attempt
- **WHEN** an unauthenticated request sends a PATCH to update an anime
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Not-found handling for update
The system SHALL return 404 when the specified anime does not exist in the watch space. Membership SHALL be checked before the existence check to prevent information leakage.

#### Scenario: Anime not found in watch space
- **WHEN** an authenticated member sends a PATCH to `/watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` and no anime with that ID exists in the watch space
- **THEN** the system SHALL return `404 Not Found`

#### Scenario: Valid anime ID but wrong watch space
- **WHEN** an authenticated member sends a PATCH where the anime ID belongs to a different watch space
- **THEN** the system SHALL return `404 Not Found`

### Requirement: Response shape matches detail endpoint
The system SHALL return the updated anime in the same response shape as `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}`, including full participant entries and watch sessions.

#### Scenario: Response includes all detail fields
- **WHEN** a PATCH returns `200 OK`
- **THEN** the response SHALL contain: `watchSpaceAnimeId`, `anilistMediaId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `format`, `season`, `seasonYear`, `sharedStatus`, `sharedEpisodesWatched`, `mood`, `vibe`, `pitch`, `addedByUserId`, `addedAtUtc`, `participants` array, and `watchSessions` array
