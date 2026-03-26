### Requirement: Picker fetches random backlog anime on init
The `bloom-backlog-picker` component SHALL call `GET /watchspaces/{spaceId}/analytics/random-pick` when initialized, using the `spaceId` input, and display the result.

#### Scenario: Successful pick with full data
- **WHEN** the component initializes and the API returns a pick with title "Spy x Family", 25 episodes, mood "Cozy", vibe "Weekend binge"
- **THEN** the component SHALL display the cover image, title "Spy x Family", "25 episodes", a mood badge "Cozy", and a vibe badge "Weekend binge"

#### Scenario: Pick with null optional fields
- **WHEN** the API returns a pick with null mood, vibe, pitch, and null cover image
- **THEN** the component SHALL display a placeholder cover, the title, the episode count, and SHALL NOT render mood/vibe/pitch badges

### Requirement: Reroll fetches a new random pick
The component SHALL provide a "Reroll" button that fetches a new random pick from the same endpoint when clicked.

#### Scenario: User clicks reroll
- **WHEN** the user clicks the "Reroll" button
- **THEN** the component SHALL show a loading state and call the random-pick endpoint again, replacing the current result with the new pick

### Requirement: Empty backlog shows message
When the API returns a null pick, the component SHALL display the message from the response.

#### Scenario: Empty backlog
- **WHEN** the API returns `pick = null` with `message = "Your backlog is empty — add some anime first!"`
- **THEN** the component SHALL display "Your backlog is empty — add some anime first!" and SHALL NOT render the pick card or reroll button

### Requirement: Loading state during API calls
The component SHALL display a loading skeleton while the random-pick API call is in flight, both on initial load and on reroll.

#### Scenario: Initial loading
- **WHEN** the component is fetching the random pick
- **THEN** the component SHALL display a skeleton placeholder approximating the pick card layout

### Requirement: View Details navigates to anime detail
The component SHALL emit a `picked` event with the `watchSpaceAnimeId` when the user clicks the "View Details" link, allowing the parent to handle navigation.

#### Scenario: User clicks View Details
- **WHEN** a pick is displayed and the user clicks "View Details"
- **THEN** the component SHALL emit the `picked` event with the `watchSpaceAnimeId` of the current pick

### Requirement: Episode count display
The component SHALL display the episode count when available, or omit it when null.

#### Scenario: Known episode count
- **WHEN** the pick has `episodeCountSnapshot = 25`
- **THEN** the component SHALL display "25 episodes"

#### Scenario: Unknown episode count
- **WHEN** the pick has `episodeCountSnapshot = null`
- **THEN** the component SHALL NOT display an episode count
