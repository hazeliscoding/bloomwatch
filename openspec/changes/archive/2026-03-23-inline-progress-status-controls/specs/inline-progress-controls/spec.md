## ADDED Requirements

### Requirement: Service method for updating shared anime state
The `WatchSpaceService` SHALL expose an `updateSharedAnime(spaceId, animeId, body)` method that sends `PATCH /watchspaces/{spaceId}/anime/{animeId}` with optional fields `sharedStatus` and `sharedEpisodesWatched`.

#### Scenario: Update shared status only
- **WHEN** `updateSharedAnime()` is called with `{ sharedStatus: "Watching" }`
- **THEN** the service SHALL send a PATCH request with `{ sharedStatus: "Watching" }` and return the updated anime detail

#### Scenario: Update shared episodes only
- **WHEN** `updateSharedAnime()` is called with `{ sharedEpisodesWatched: 5 }`
- **THEN** the service SHALL send a PATCH request with `{ sharedEpisodesWatched: 5 }` and return the updated anime detail

#### Scenario: Update both fields
- **WHEN** `updateSharedAnime()` is called with `{ sharedStatus: "Finished", sharedEpisodesWatched: 24 }`
- **THEN** the service SHALL send a PATCH request with both fields and return the updated anime detail

### Requirement: Optimistic UI updates with rollback
All inline control mutations SHALL update the local UI state immediately before the API response arrives. On API failure, the system SHALL revert to the previous value and display an inline error.

#### Scenario: Successful optimistic update
- **WHEN** a member changes a shared status dropdown value
- **THEN** the UI SHALL reflect the new status immediately without waiting for the API response
- **AND** the API call SHALL fire in the background

#### Scenario: API failure triggers rollback
- **WHEN** a member changes a control value and the API call returns an error
- **THEN** the UI SHALL revert the control to its previous value
- **AND** an inline error message SHALL appear near the control

#### Scenario: Inline error auto-dismisses
- **WHEN** an inline error message is displayed after a failed mutation
- **THEN** the error message SHALL fade out after 3 seconds

### Requirement: Episode stepper uses switchMap cancellation
When the episode stepper is clicked multiple times in quick succession, only the latest value SHALL be persisted to the backend.

#### Scenario: Rapid increment clicks
- **WHEN** a member clicks the "+" button 3 times quickly (e.g. from 5 to 8)
- **THEN** the UI SHALL show 6, 7, 8 in sequence
- **AND** only the final value (8) SHALL be persisted to the backend (earlier in-flight requests are cancelled)

### Requirement: Episode count validation
The system SHALL prevent episode counts from exceeding the known total or going below zero.

#### Scenario: Increment beyond episode count
- **WHEN** a member attempts to increment shared episodes beyond `episodeCountSnapshot`
- **THEN** the "+" button SHALL be disabled and no API call SHALL be made

#### Scenario: Decrement below zero
- **WHEN** a member attempts to decrement shared episodes below 0
- **THEN** the "−" button SHALL be disabled and no API call SHALL be made

#### Scenario: Unknown episode count
- **WHEN** `episodeCountSnapshot` is null (ongoing series)
- **THEN** the "+" button SHALL remain enabled with no upper bound enforced client-side
