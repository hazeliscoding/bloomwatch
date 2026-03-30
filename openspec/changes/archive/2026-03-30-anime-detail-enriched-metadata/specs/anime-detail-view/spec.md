## MODIFIED Requirements

### Requirement: Anime detail page loads and displays full anime metadata
The system SHALL render the `AnimeDetail` component at route `/watch-spaces/:id/anime/:animeId` showing a hero section with the anime's cover image, preferred title, format, season/year, episode count, airing status, genres, AniList tags, description (as sanitized HTML), AniList score, popularity, and an external AniList link, fetched from `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}`.

#### Scenario: Successful detail load
- **WHEN** a user navigates to `/watch-spaces/{id}/anime/{animeId}` and the API returns `200 OK`
- **THEN** the component SHALL display the anime's `preferredTitle`, `coverImageUrlSnapshot` (as a hero image), `format`, `season`, `seasonYear`, `episodeCountSnapshot`, `airingStatus` (as a colored badge), `genres` (as badges), `tags` (as badges with spoiler handling), `description` (as sanitized HTML), `anilistScore`, `anilistPopularity`, and an external AniList link

#### Scenario: Cover image fallback
- **WHEN** `coverImageUrlSnapshot` is `null` or the image fails to load
- **THEN** the component SHALL display a placeholder gradient in the hero image area

#### Scenario: Loading state
- **WHEN** the detail API request is in flight
- **THEN** the component SHALL display a loading indicator

#### Scenario: Error state
- **WHEN** the detail API request fails (network error, 4xx, 5xx)
- **THEN** the component SHALL display an error message with a retry button

### Requirement: TypeScript interfaces for detail response DTOs
The `watch-space.model.ts` file SHALL define interfaces matching the `GetWatchSpaceAnimeDetailResult` API response shape.

#### Scenario: WatchSpaceAnimeDetail interface
- **WHEN** the detail API returns a response
- **THEN** the response SHALL be typed as `WatchSpaceAnimeDetail` with fields: `watchSpaceAnimeId`, `anilistMediaId`, `preferredTitle`, `coverImageUrlSnapshot`, `episodeCountSnapshot`, `format`, `season`, `seasonYear`, `sharedStatus`, `sharedEpisodesWatched`, `mood`, `vibe`, `pitch`, `addedByUserId`, `addedAtUtc`, `participants` (array of `ParticipantDetail`), `genres` (optional string array), `description` (optional string), `anilistScore` (optional number), `anilistPopularity` (optional number), `tags` (optional `AnimeTag` array), `siteUrl` (optional string), `airingStatus` (optional string)

#### Scenario: AnimeTag interface
- **WHEN** the detail API returns tag data
- **THEN** each tag SHALL be typed as `AnimeTag` with fields: `name` (string), `rank` (number), `isMediaSpoiler` (boolean)

#### Scenario: ParticipantDetail interface
- **WHEN** a participant entry is received
- **THEN** it SHALL be typed as `ParticipantDetail` with fields: `userId`, `individualStatus`, `episodesWatched`, `ratingScore` (nullable), `ratingNotes` (nullable), `lastUpdatedAtUtc`
