## ADDED Requirements

### Requirement: Add anime to a watch space via API
The system SHALL expose a `POST /watchspaces/{watchSpaceId}/anime` endpoint that accepts a JSON body with `anilistMediaId` (required integer), `mood` (optional string), `vibe` (optional string), and `pitch` (optional string). The endpoint SHALL return 201 Created on success with the new `watchSpaceAnimeId` and a metadata snapshot.

#### Scenario: Successful anime addition
- **WHEN** an authenticated member of the watch space sends `POST /watchspaces/{id}/anime` with a valid `anilistMediaId` that is cached in the media cache and not already in the watch space
- **THEN** the system creates a `WatchSpaceAnime` record with `sharedStatus = Backlog`, `sharedEpisodesWatched = 0`, and metadata snapshots (`preferredTitle`, `episodeCountSnapshot`, `coverImageUrlSnapshot`, `format`, `season`, `seasonYear`) from the cached media entry, creates a `ParticipantEntry` for the adding user with `individualStatus = Backlog` and `episodesWatched = 0`, sets `addedByUserId` to the authenticated user's ID, and returns 201 with the `watchSpaceAnimeId` and metadata snapshot

#### Scenario: Successful addition with optional metadata
- **WHEN** an authenticated member sends the request with `mood`, `vibe`, and `pitch` fields populated
- **THEN** the system stores the `mood`, `vibe`, and `pitch` values on the `WatchSpaceAnime` record alongside all other fields

### Requirement: Membership verification for adding anime
The system SHALL verify that the requesting user is a member of the target watch space before allowing anime addition.

#### Scenario: Non-member attempts to add anime
- **WHEN** an authenticated user who is NOT a member of the watch space sends `POST /watchspaces/{id}/anime`
- **THEN** the system returns 403 Forbidden and does not create any records

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated user sends `POST /watchspaces/{id}/anime`
- **THEN** the system returns 401 Unauthorized

### Requirement: Duplicate anime prevention
The system SHALL prevent the same AniList media ID from being added to a watch space more than once.

#### Scenario: Duplicate AniList media ID in same watch space
- **WHEN** a member sends `POST /watchspaces/{id}/anime` with an `anilistMediaId` that already exists in that watch space
- **THEN** the system returns 409 Conflict and does not create a new record

#### Scenario: Same anime in different watch spaces
- **WHEN** a member adds an anime with `anilistMediaId = 154587` to watch space A, and another member adds the same `anilistMediaId` to watch space B
- **THEN** both additions succeed because the uniqueness constraint is scoped to `(watchSpaceId, anilistMediaId)`

### Requirement: AniList metadata snapshotting
The system SHALL snapshot metadata from the local AniList media cache at the time of addition. The preferred title SHALL be resolved as: `TitleEnglish` if available, otherwise `TitleRomaji`, otherwise `TitleNative`.

#### Scenario: Media found in cache with English title
- **WHEN** the media cache contains an entry with `TitleEnglish = "Frieren: Beyond Journey's End"`, `TitleRomaji = "Sousou no Frieren"`, `Episodes = 28`, `CoverImageUrl = "https://..."`, `Format = "TV"`, `Season = "FALL"`, `SeasonYear = 2023`
- **THEN** the system snapshots `preferredTitle = "Frieren: Beyond Journey's End"`, `episodeCountSnapshot = 28`, `coverImageUrlSnapshot = "https://..."`, `format = "TV"`, `season = "FALL"`, `seasonYear = 2023`

#### Scenario: Media not found in cache
- **WHEN** a member sends `POST /watchspaces/{id}/anime` with an `anilistMediaId` that does not exist in the local media cache
- **THEN** the system returns 404 Not Found

### Requirement: Watch space existence validation
The system SHALL validate that the target watch space exists.

#### Scenario: Non-existent watch space
- **WHEN** a request is sent to `POST /watchspaces/{id}/anime` where `{id}` does not correspond to any watch space
- **THEN** the system returns 404 Not Found

### Requirement: WatchSpaceAnime aggregate creation invariants
The `WatchSpaceAnime` aggregate SHALL always be created with exactly one `ParticipantEntry` for the adding user. The `addedAtUtc` timestamp SHALL be set to the current UTC time at creation.

#### Scenario: Initial participant entry created on add
- **WHEN** a member successfully adds an anime to the watch space
- **THEN** the resulting `WatchSpaceAnime` aggregate contains exactly one `ParticipantEntry` with `userId` matching the adding user, `individualStatus = Backlog`, and `episodesWatched = 0`
