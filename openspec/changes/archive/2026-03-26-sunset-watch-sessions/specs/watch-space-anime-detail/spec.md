## REMOVED Requirements

### Requirement: Response includes watch sessions
**Reason**: Watch sessions feature is being sunset. The `watchSessions` array is no longer populated or returned.
**Migration**: Clients should remove any logic that reads the `watchSessions` field from the anime detail response. Episode tracking is covered by `sharedEpisodesWatched` and per-participant `episodesWatched`.

### Requirement: No watch sessions recorded yet
**Reason**: Empty-state handling for watch sessions is no longer needed since the field is removed.
**Migration**: None required — the `watchSessions` field no longer exists in the response.

## MODIFIED Requirements

### Requirement: Get anime detail in a watch space
The system SHALL expose `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` as an authenticated, member-only endpoint that returns the full detail for a single anime tracked in the specified watch space.

#### Scenario: Successful detail fetch
- **WHEN** an authenticated member of watch space `{watchSpaceId}` sends `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` and the anime exists in that watch space
- **THEN** the system SHALL return `200 OK` with a JSON object containing: `watchSpaceAnimeId`, `anilistMediaId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `format`, `season`, `seasonYear`, `sharedStatus`, `sharedEpisodesWatched`, `mood`, `vibe`, `pitch`, `addedByUserId`, `addedAtUtc`

#### Scenario: Response includes full participant entries
- **WHEN** the detail endpoint returns `200 OK`
- **THEN** the response SHALL include a `participants` array where each entry contains: `userId`, `individualStatus`, `episodesWatched`, `ratingScore`, `ratingNotes`, `lastUpdatedAtUtc`
