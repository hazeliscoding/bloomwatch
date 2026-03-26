## REMOVED Requirements

### Requirement: Anime detail page displays watch session history
**Reason**: Watch sessions feature is being sunset. The chronological session list is no longer displayed.
**Migration**: None — the UI section is removed entirely.

### Requirement: User can record a watch session
**Reason**: Watch sessions feature is being sunset. The inline form and modal for logging sessions are removed.
**Migration**: None — episode tracking is covered by participant progress updates.

## MODIFIED Requirements

### Requirement: Service methods for anime detail operations
The `WatchSpaceService` SHALL expose methods for fetching anime detail and performing participant mutation operations.

#### Scenario: getAnimeDetail method
- **WHEN** `getAnimeDetail(spaceId, animeId)` is called
- **THEN** it SHALL send `GET /watchspaces/{spaceId}/anime/{animeId}` and return an `Observable<WatchSpaceAnimeDetail>`

#### Scenario: updateParticipantProgress method
- **WHEN** `updateParticipantProgress(spaceId, animeId, body)` is called
- **THEN** it SHALL send `PATCH /watchspaces/{spaceId}/anime/{animeId}/participant-progress` with the request body

#### Scenario: updateParticipantRating method
- **WHEN** `updateParticipantRating(spaceId, animeId, body)` is called
- **THEN** it SHALL send `PATCH /watchspaces/{spaceId}/anime/{animeId}/participant-rating` with the request body

### Requirement: TypeScript interfaces for detail response DTOs
The `watch-space.model.ts` file SHALL define interfaces matching the `GetWatchSpaceAnimeDetailResult` API response shape.

#### Scenario: WatchSpaceAnimeDetail interface
- **WHEN** the detail API returns a response
- **THEN** the response SHALL be typed as `WatchSpaceAnimeDetail` with fields: `watchSpaceAnimeId`, `anilistMediaId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `format`, `season`, `seasonYear`, `sharedStatus`, `sharedEpisodesWatched`, `mood`, `vibe`, `pitch`, `addedByUserId`, `addedAtUtc`, `participants` (array of `ParticipantDetail`)

#### Scenario: ParticipantDetail interface
- **WHEN** a participant entry is received
- **THEN** it SHALL be typed as `ParticipantDetail` with fields: `userId`, `individualStatus`, `episodesWatched`, `ratingScore` (nullable), `ratingNotes` (nullable), `lastUpdatedAtUtc`
