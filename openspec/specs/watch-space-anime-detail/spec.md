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
