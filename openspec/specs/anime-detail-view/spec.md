### Requirement: Anime detail page loads and displays full anime metadata
The system SHALL render the `AnimeDetail` component at route `/watch-spaces/:id/anime/:animeId` showing a hero section with the anime's cover image, preferred title, format, season/year, and episode count, fetched from `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}`.

#### Scenario: Successful detail load
- **WHEN** a user navigates to `/watch-spaces/{id}/anime/{animeId}` and the API returns `200 OK`
- **THEN** the component SHALL display the anime's `preferredTitle`, `coverImageUrlSnapshot` (as a hero image), `format`, `season`, `seasonYear`, and `episodeCountSnapshot`

#### Scenario: Cover image fallback
- **WHEN** `coverImageUrlSnapshot` is `null` or the image fails to load
- **THEN** the component SHALL display a placeholder gradient in the hero image area

#### Scenario: Loading state
- **WHEN** the detail API request is in flight
- **THEN** the component SHALL display a loading indicator

#### Scenario: Error state
- **WHEN** the detail API request fails (network error, 4xx, 5xx)
- **THEN** the component SHALL display an error message with a retry button

### Requirement: Anime detail page displays shared tracking state
The system SHALL display the shared tracking state section showing the anime's shared status, shared episodes watched, mood, vibe, and pitch.

#### Scenario: Shared state rendering
- **WHEN** the detail data has loaded successfully
- **THEN** the component SHALL display `sharedStatus` as a colored badge, `sharedEpisodesWatched` as a progress indicator (with `episodeCountSnapshot` as denominator when available), and `mood`, `vibe`, and `pitch` text when present

#### Scenario: Optional fields absent
- **WHEN** `mood`, `vibe`, or `pitch` is `null`
- **THEN** the component SHALL omit those fields from the display rather than showing empty labels

### Requirement: Anime detail page displays participant progress table
The system SHALL display a participants section listing every participant's individual status, episodes watched, rating score, and rating notes.

#### Scenario: Multiple participants
- **WHEN** the `participants` array contains two or more entries
- **THEN** the component SHALL render a row for each participant showing `displayName` (resolved from members), `individualStatus` as a badge, `episodesWatched`, `ratingScore` (if present), and `ratingNotes` (if present)

#### Scenario: No rating submitted
- **WHEN** a participant's `ratingScore` is `null`
- **THEN** the component SHALL display a "No rating" placeholder for that participant

#### Scenario: Participant display name resolution
- **WHEN** rendering participant entries
- **THEN** the component SHALL resolve user IDs to display names using the watch space member data

### Requirement: User can update their own participant progress
The system SHALL provide an inline form allowing the current user to update their individual status and episodes watched via `PATCH /watchspaces/{id}/anime/{animeId}/participant-progress`.

#### Scenario: Progress form submission
- **WHEN** the user selects a new status and/or episode count and submits the progress form
- **THEN** the component SHALL send a `PATCH` request with `{ status, episodesWatched }` and refresh the detail data on success

#### Scenario: Progress form validation
- **WHEN** the user attempts to submit with `episodesWatched` less than 0
- **THEN** the form SHALL prevent submission

#### Scenario: Progress update error
- **WHEN** the `PATCH` request fails
- **THEN** the component SHALL display an error message without modifying the displayed data

### Requirement: User can submit or update their rating
The system SHALL provide an inline form allowing the current user to submit or update their rating (0.5–10.0 in 0.5 increments) and optional notes via `PATCH /watchspaces/{id}/anime/{animeId}/participant-rating`.

#### Scenario: Rating form submission
- **WHEN** the user enters a score between 0.5 and 10.0 (in 0.5 increments) and optional notes text, then submits the rating form
- **THEN** the component SHALL send a `PATCH` request with `{ ratingScore, ratingNotes }` and refresh the detail data on success

#### Scenario: Rating score validation
- **WHEN** the user enters a score outside the range 0.5–10.0 or not in 0.5 increments
- **THEN** the form SHALL prevent submission

#### Scenario: Rating notes length limit
- **WHEN** the user enters notes exceeding 1000 characters
- **THEN** the form SHALL prevent submission and indicate the character limit

### Requirement: Back navigation to watch space
The system SHALL provide a visible back navigation element that returns the user to the parent watch space detail page.

#### Scenario: Back link navigates correctly
- **WHEN** the user clicks the back navigation element
- **THEN** the router SHALL navigate to `/watch-spaces/{watchSpaceId}`

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
