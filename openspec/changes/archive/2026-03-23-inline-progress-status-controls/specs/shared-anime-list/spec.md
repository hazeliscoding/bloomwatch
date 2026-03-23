## ADDED Requirements

### Requirement: Inline shared status dropdown on anime list cards
Each anime card in the list view SHALL display a `<select>` dropdown for `sharedStatus` that allows members to change the shared status without navigating to the detail page.

#### Scenario: Status dropdown renders with current value
- **WHEN** the anime list renders a card for an anime with `sharedStatus = "Backlog"`
- **THEN** the card SHALL display a dropdown with "Backlog" selected and options for all valid statuses: Backlog, Watching, Finished, Paused, Dropped

#### Scenario: Changing status from the list card
- **WHEN** a member selects "Watching" from the status dropdown on an anime card
- **THEN** the system SHALL call `PATCH /watchspaces/{id}/anime/{animeId}` with `{ sharedStatus: "Watching" }`
- **AND** the card SHALL update its status badge and dropdown value immediately (optimistic)
- **AND** the tab counts SHALL update to reflect the status change

#### Scenario: Status change failure on list card
- **WHEN** a member changes the status dropdown and the API call fails
- **THEN** the dropdown SHALL revert to the previous value
- **AND** an inline error message SHALL appear on the card

### Requirement: Participant episode increment button wired to backend
The existing "+" button on each participant row in the anime list card SHALL call the participant progress endpoint to persist the increment.

#### Scenario: Increment own episode count from list
- **WHEN** a member clicks the "+" button next to their own participant row on an anime card
- **THEN** the system SHALL call `PATCH /watchspaces/{id}/anime/{animeId}/participant-progress` with `{ episodesWatched: currentEpisodes + 1, individualStatus: currentStatus }`
- **AND** the participant's episode count on the card SHALL update immediately (optimistic)

#### Scenario: Increment fails due to exceeding episode count
- **WHEN** a member clicks "+" and the new episode count would exceed `episodeCountSnapshot`
- **THEN** the system SHALL NOT make an API call
- **AND** the "+" button SHALL be disabled for that participant row

#### Scenario: Increment fails due to API error
- **WHEN** a member clicks "+" and the API call returns an error
- **THEN** the episode count SHALL revert to the previous value
- **AND** an inline error message SHALL appear on the card

#### Scenario: Increment for another user's row
- **WHEN** a member views another participant's row on a list card
- **THEN** the "+" button SHALL only be visible for the current user's own participant row

## MODIFIED Requirements

### Requirement: Anime card displays participant progress
Each anime card SHALL display the individual progress of every participant in the watch space for that anime. The current user's participant row SHALL include an interactive "+" button to increment their episode count. Other participants' rows SHALL be read-only.

#### Scenario: Multiple participants with varying progress
- **WHEN** an anime has multiple participants with different episode counts
- **THEN** each participant's display name and episode count SHALL be shown on the anime card

#### Scenario: Anime has no episode count snapshot
- **WHEN** an anime's `episodeCountSnapshot` is null (e.g. ongoing series with unknown total)
- **THEN** the episode progress SHALL display without a total (e.g. "Ep 5" instead of "Ep 5 / 24")

#### Scenario: Current user's row shows increment control
- **WHEN** the anime card renders the current user's participant row
- **THEN** a "+" button SHALL appear next to their episode count
- **AND** clicking it SHALL increment and persist the episode count

#### Scenario: Other users' rows are read-only
- **WHEN** the anime card renders another participant's row
- **THEN** no interactive controls SHALL be shown for that row

### Requirement: List refreshes after adding anime
The anime list SHALL refresh its data when the anime search modal is closed after an anime has been added.

#### Scenario: Anime added via search modal
- **WHEN** a member adds an anime via the search modal and the modal closes
- **THEN** the anime list SHALL re-fetch from the API and display the updated list including the newly added anime

#### Scenario: Search modal closed without adding
- **WHEN** a member closes the search modal without adding any anime
- **THEN** the anime list SHALL NOT re-fetch (no unnecessary network call)

#### Scenario: Inline update does not trigger full refresh
- **WHEN** a member updates a status or increments episodes inline on a list card
- **THEN** the system SHALL update only the affected card's local data without re-fetching the entire list
