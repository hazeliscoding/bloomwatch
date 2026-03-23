### Requirement: Get anime detail in a watch space
The system SHALL expose `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` as an authenticated, member-only endpoint that returns the full detail for a single anime tracked in the specified watch space.

#### Scenario: Successful detail fetch
- **WHEN** an authenticated member of watch space `{watchSpaceId}` sends `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` and the anime exists in that watch space
- **THEN** the system SHALL return `200 OK` with a JSON object containing: `watchSpaceAnimeId`, `anilistMediaId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `format`, `season`, `seasonYear`, `sharedStatus`, `sharedEpisodesWatched`, `mood`, `vibe`, `pitch`, `addedByUserId`, `addedAtUtc`

#### Scenario: Response includes full participant entries
- **WHEN** the detail endpoint returns `200 OK`
- **THEN** the response SHALL include a `participants` array where each entry contains: `userId`, `individualStatus`, `episodesWatched`, `ratingScore`, `ratingNotes`, `lastUpdatedAtUtc`

#### Scenario: Response includes watch sessions
- **WHEN** the detail endpoint returns `200 OK`
- **THEN** the response SHALL include a `watchSessions` array where each entry contains: `watchSessionId`, `sessionDateUtc`, `startEpisode`, `endEpisode`, `notes`, `createdByUserId`

#### Scenario: No watch sessions recorded yet
- **WHEN** the detail endpoint returns `200 OK` and no watch sessions have been recorded for this anime
- **THEN** the `watchSessions` array SHALL be empty

### Requirement: Membership enforcement for anime detail
The system SHALL verify that the requesting user is a member of the watch space before returning detail data.

#### Scenario: Non-member access
- **WHEN** an authenticated user who is NOT a member of watch space `{watchSpaceId}` sends `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}`
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated access
- **WHEN** an unauthenticated request is sent to `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}`
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Not-found handling for anime detail
The system SHALL return 404 when the requested anime does not exist in the specified watch space. Membership SHALL be checked before the existence check to prevent information leakage.

#### Scenario: Anime not tracked in watch space
- **WHEN** an authenticated member sends `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` and no anime with that ID exists in the watch space
- **THEN** the system SHALL return `404 Not Found`

#### Scenario: Valid anime ID but wrong watch space
- **WHEN** an authenticated member sends `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` where the anime ID belongs to a different watch space
- **THEN** the system SHALL return `404 Not Found`

### Requirement: WatchSession entity in the domain
The system SHALL define a `WatchSession` entity as part of the `WatchSpaceAnime` aggregate with fields: `watchSessionId` (Guid), `watchSpaceAnimeId`, `sessionDateUtc` (DateTime), `startEpisode` (int), `endEpisode` (int), `notes` (string, nullable), `createdByUserId` (Guid).

#### Scenario: WatchSession table exists in database
- **WHEN** the EF migration for this change is applied
- **THEN** a `watch_sessions` table SHALL exist with columns for all `WatchSession` fields and a foreign key to `watch_space_anime(id)` with cascade delete

#### Scenario: WatchSessions loaded with aggregate
- **WHEN** a `WatchSpaceAnime` is fetched for the detail endpoint
- **THEN** its `WatchSessions` collection SHALL be eagerly loaded

### Requirement: Shared status dropdown persists to backend
The shared status dropdown on the anime detail page SHALL call `PATCH /watchspaces/{id}/anime/{animeId}` with `{ sharedStatus }` when the value changes.

#### Scenario: Member changes shared status
- **WHEN** a member selects a new value from the shared status dropdown on the detail page
- **THEN** the system SHALL optimistically update the local signal
- **AND** call `updateSharedAnime()` with `{ sharedStatus: newValue }`
- **AND** on success, the detail page SHALL reflect the persisted status

#### Scenario: Shared status change fails
- **WHEN** a member changes the shared status and the API returns an error
- **THEN** the dropdown SHALL revert to the previous value
- **AND** an inline error message SHALL appear below the shared status section

### Requirement: Shared episode stepper persists to backend
The shared episode stepper (+/− buttons) on the anime detail page SHALL call `PATCH /watchspaces/{id}/anime/{animeId}` with `{ sharedEpisodesWatched }` when the value changes.

#### Scenario: Member increments shared episodes
- **WHEN** a member clicks the "+" button on the shared episode stepper
- **THEN** the displayed count SHALL increment by 1 immediately
- **AND** the system SHALL call `updateSharedAnime()` with the new `sharedEpisodesWatched` value

#### Scenario: Member decrements shared episodes
- **WHEN** a member clicks the "−" button on the shared episode stepper
- **THEN** the displayed count SHALL decrement by 1 immediately
- **AND** the system SHALL call `updateSharedAnime()` with the new `sharedEpisodesWatched` value

#### Scenario: Shared episode stepper at boundary
- **WHEN** `sharedEpisodesWatched` is 0
- **THEN** the "−" button SHALL be disabled
- **WHEN** `sharedEpisodesWatched` equals `episodeCountSnapshot` (and it is not null)
- **THEN** the "+" button SHALL be disabled

#### Scenario: Rapid stepper clicks use latest value
- **WHEN** a member clicks "+" multiple times in quick succession
- **THEN** the UI SHALL show each intermediate value
- **AND** only the final value SHALL be sent to the backend (via switchMap cancellation)

#### Scenario: Shared episode update fails
- **WHEN** the episode stepper API call fails
- **THEN** the episode count SHALL revert to the last successfully persisted value
- **AND** an inline error message SHALL appear below the stepper

### Requirement: Progress bar reflects persisted shared episodes
The shared progress bar on the anime detail page SHALL update in sync with the shared episode stepper value.

#### Scenario: Progress bar updates on increment
- **WHEN** a member increments the shared episode count
- **THEN** the progress bar fill percentage SHALL update to `(sharedEpisodesWatched / episodeCountSnapshot) * 100`

#### Scenario: Progress bar with unknown total
- **WHEN** `episodeCountSnapshot` is null
- **THEN** the progress bar SHALL NOT be displayed (or show an indeterminate state)
