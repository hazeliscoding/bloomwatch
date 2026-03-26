## MODIFIED Requirements

### Requirement: Compatibility score display
The dashboard SHALL render the compatibility and backlog picker sections in a two-column layout. The left column SHALL contain the `bloom-compat-ring` component inside a card, and the right column SHALL contain the `bloom-backlog-picker` component inside a highlighted card. When a pick is selected, clicking "View Details" SHALL navigate to the anime detail page.

#### Scenario: Compatibility with valid score and picker with result
- **WHEN** the dashboard response contains a non-null `compatibility` object and the random-pick endpoint returns a pick
- **THEN** the dashboard SHALL render the compatibility ring in the left column and the backlog picker with the random result in the right column

#### Scenario: Compatibility is null and backlog is empty
- **WHEN** the dashboard response contains `compatibility = null` and the random-pick endpoint returns null pick
- **THEN** the dashboard SHALL render the `bloom-compat-ring` placeholder in the left column and the picker empty state in the right column

#### Scenario: Picker view details navigates to anime
- **WHEN** the user clicks "View Details" on the backlog picker
- **THEN** the system SHALL navigate to `/watch-spaces/:id/anime/:animeId`
