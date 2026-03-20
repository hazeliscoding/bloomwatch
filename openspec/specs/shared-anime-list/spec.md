### Requirement: Anime list renders on the watch space detail page

The system SHALL display an anime list section on the watch space detail page at `/watch-spaces/:id`. The list SHALL show all anime that have been added to the watch space, fetched from `GET /watchspaces/{watchSpaceId}/anime`.

#### Scenario: Watch space has tracked anime

- **WHEN** a member navigates to `/watch-spaces/:id` and the watch space contains anime
- **THEN** the system SHALL display a list of anime cards below the space header and "Add Anime" button
- **AND** each card SHALL show: cover image, preferred title, shared status badge, shared episode progress (e.g. "Ep 5 / 24"), and each participant's display name with their individual episode count

#### Scenario: Watch space has no anime

- **WHEN** a member navigates to `/watch-spaces/:id` and the watch space contains no anime
- **THEN** the system SHALL display a friendly empty state message (e.g. "No anime yet — add one to get started!")

#### Scenario: Anime list is loading

- **WHEN** the anime list data is being fetched
- **THEN** the system SHALL display a loading indicator

#### Scenario: Anime list fails to load

- **WHEN** the anime list request fails
- **THEN** the system SHALL display an error message with an option to retry

### Requirement: Status filter tabs

The system SHALL provide tab controls to filter the anime list by shared status. The available tabs SHALL be: All, Backlog, Watching, Finished, Paused, Dropped.

#### Scenario: Default tab is "All"

- **WHEN** the anime list first loads
- **THEN** the "All" tab SHALL be selected and all anime SHALL be displayed regardless of shared status

#### Scenario: Selecting a status tab filters the list

- **WHEN** a member selects a status tab (e.g. "Watching")
- **THEN** the list SHALL display only anime whose `sharedStatus` matches the selected tab value
- **AND** the selected tab SHALL be visually highlighted

#### Scenario: Empty state for a filtered tab

- **WHEN** a member selects a status tab and no anime match that status
- **THEN** the system SHALL display a tab-specific empty message (e.g. "Nothing in Backlog yet")

### Requirement: Anime card displays participant progress

Each anime card SHALL display the individual progress of every participant in the watch space for that anime.

#### Scenario: Multiple participants with varying progress

- **WHEN** an anime has multiple participants with different episode counts
- **THEN** each participant's display name and episode count SHALL be shown on the anime card

#### Scenario: Anime has no episode count snapshot

- **WHEN** an anime's `episodeCountSnapshot` is null (e.g. ongoing series with unknown total)
- **THEN** the episode progress SHALL display without a total (e.g. "Ep 5" instead of "Ep 5 / 24")

### Requirement: Clicking an anime card navigates to the detail page

The system SHALL navigate to the anime detail page when a member clicks an anime card.

#### Scenario: Navigation to anime detail

- **WHEN** a member clicks on an anime card
- **THEN** the system SHALL navigate to `/watch-spaces/:id/anime/:watchSpaceAnimeId`

### Requirement: List refreshes after adding anime

The anime list SHALL refresh its data when the anime search modal is closed after an anime has been added.

#### Scenario: Anime added via search modal

- **WHEN** a member adds an anime via the search modal and the modal closes
- **THEN** the anime list SHALL re-fetch from the API and display the updated list including the newly added anime

#### Scenario: Search modal closed without adding

- **WHEN** a member closes the search modal without adding any anime
- **THEN** the anime list SHALL NOT re-fetch (no unnecessary network call)
