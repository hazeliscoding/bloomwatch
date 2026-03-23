## ADDED Requirements

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
