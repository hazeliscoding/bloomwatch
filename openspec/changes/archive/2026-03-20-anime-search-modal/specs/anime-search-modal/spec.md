## ADDED Requirements

### Requirement: Search modal can be opened from watch space detail
The system SHALL provide a trigger (button) on the watch space detail page that opens the anime search modal. The modal SHALL be an overlay dialog using the `bloom-modal` component.

#### Scenario: User clicks Add Anime button
- **WHEN** the user is on a watch space detail page
- **AND** clicks the "Add Anime" button
- **THEN** the anime search modal SHALL open

### Requirement: User can search anime by title
The system SHALL provide a text input inside the modal that searches anime via the backend `GET /api/anilist/search?query=<term>` endpoint. The search SHALL be debounced by 300ms to avoid excessive API calls.

#### Scenario: User types a search query
- **WHEN** the user types "Cowboy" into the search input
- **AND** 300ms passes without further typing
- **THEN** the system SHALL call the search endpoint with `query=Cowboy`
- **AND** display the results below the input

#### Scenario: User clears the search input
- **WHEN** the user clears the search input
- **THEN** the results list SHALL be cleared
- **AND** no API call SHALL be made

### Requirement: Search results display anime metadata
Each search result item SHALL display: cover image, title (English preferred, romaji fallback), format badge, episode count, season/year, and genre badges.

#### Scenario: Results are displayed
- **WHEN** the search returns results
- **THEN** each result SHALL show the cover image, preferred title, format, episode count (if available), season and year (if available), and up to 3 genre badges

#### Scenario: Title fallback
- **WHEN** a result has no English title
- **THEN** the romaji title SHALL be displayed instead

### Requirement: Loading state during search
The system SHALL display a loading indicator while a search request is in flight.

#### Scenario: Search is in progress
- **WHEN** a search request has been sent but the response has not arrived
- **THEN** the modal SHALL display a loading indicator in the results area

### Requirement: Empty state when no results
The system SHALL display a message when a search returns zero results.

#### Scenario: No results found
- **WHEN** a search completes with zero results
- **THEN** the modal SHALL display an empty state message (e.g., "No anime found")

### Requirement: Error state on search failure
The system SHALL display an error message when the search request fails.

#### Scenario: Search request fails
- **WHEN** the search API call returns an error
- **THEN** the modal SHALL display an error message with an option to retry

### Requirement: User can add anime to watch space
Each search result SHALL have an "Add" action. Clicking it SHALL send a `POST /watchspaces/{id}/anime` request with the selected anime's AniList media ID and metadata.

#### Scenario: User adds an anime
- **WHEN** the user clicks the "Add" button on a search result
- **THEN** the system SHALL send an add-anime request to the backend
- **AND** the button SHALL show a loading state during the request

#### Scenario: Anime added successfully
- **WHEN** the add request succeeds
- **THEN** the system SHALL indicate success on that result item (e.g., checkmark or "Added" label)
- **AND** the modal SHALL remain open so the user can add more anime

#### Scenario: Add request fails
- **WHEN** the add request fails
- **THEN** the system SHALL display an error message on that result item

### Requirement: Modal closes and refreshes the anime list
When the user closes the modal after adding anime, the watch space detail page SHALL refresh its anime list to reflect newly added entries.

#### Scenario: User closes modal after adding anime
- **WHEN** the user closes the anime search modal
- **AND** at least one anime was successfully added during the session
- **THEN** the watch space detail page SHALL reload its anime list

### Requirement: Search input receives focus on modal open
When the modal opens, the search input SHALL automatically receive keyboard focus so the user can start typing immediately.

#### Scenario: Modal opens
- **WHEN** the anime search modal opens
- **THEN** the search text input SHALL have keyboard focus
