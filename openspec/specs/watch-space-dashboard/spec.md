### Requirement: Dashboard page loads summary data
The system SHALL render a dashboard page at `/watch-spaces/:id/dashboard` that fetches `GET /watchspaces/{id}/dashboard` on load and displays the composite response including stat cards, compatibility, currently-watching list, backlog highlights, and rating-gap highlights.

#### Scenario: Successful dashboard load with full data
- **WHEN** a member navigates to `/watch-spaces/:id/dashboard` and the API returns a complete dashboard response
- **THEN** the page SHALL render all five sections: stat cards, compatibility score, currently watching grid, backlog highlights grid, and rating gap highlights list

#### Scenario: Dashboard load for empty watch space
- **WHEN** a member navigates to the dashboard and the API returns stats with all zeroes, null compatibility, and empty arrays
- **THEN** the page SHALL render the stat cards with zero values and show contextual empty messages for currently watching, backlog, and rating gap sections

### Requirement: Dashboard is the default watch space view
The system SHALL route `/watch-spaces/:id` to the dashboard view. The existing anime list and settings view SHALL be accessible at `/watch-spaces/:id/manage`.

#### Scenario: Navigating to watch space root
- **WHEN** a user navigates to `/watch-spaces/:id`
- **THEN** the system SHALL redirect to `/watch-spaces/:id/dashboard`

#### Scenario: Navigating to manage view
- **WHEN** a user navigates to `/watch-spaces/:id/manage`
- **THEN** the system SHALL render the existing anime list and settings page (WatchSpaceDetail)

### Requirement: Stat cards display
The dashboard SHALL render a row of 4 stat cards showing: Total Shows, Currently Watching, Finished, and Episodes Watched Together, using the values from `stats` in the dashboard response.

#### Scenario: Stats with data
- **WHEN** the dashboard response contains `stats.totalShows = 27`, `stats.currentlyWatching = 3`, `stats.finished = 11`, `stats.episodesWatchedTogether = 184`
- **THEN** the stat cards SHALL display "27" for Total Shows, "3" for Currently Watching, "11" for Finished, and "184" for Episodes Together

#### Scenario: Stats with all zeroes
- **WHEN** the dashboard response contains all zero stats
- **THEN** the stat cards SHALL display "0" for each value

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

### Requirement: Currently watching grid
The dashboard SHALL render a grid of up to 5 anime with `sharedStatus = Watching`. Each card SHALL display the cover image (or a placeholder), the preferred title, a progress bar showing `sharedEpisodesWatched / episodeCountSnapshot`, and an episode count label. Cards SHALL link to the anime detail page.

#### Scenario: Currently watching with data
- **WHEN** the dashboard response contains 3 currently-watching entries
- **THEN** the grid SHALL render 3 cards with cover images, titles, progress bars, and episode labels

#### Scenario: Currently watching with null episode count
- **WHEN** a currently-watching entry has `episodeCountSnapshot = null`
- **THEN** the progress bar SHALL not be rendered for that card, and the episode label SHALL show only the watched count (e.g., "Ep 5")

#### Scenario: No currently watching anime
- **WHEN** the `currentlyWatching` array is empty
- **THEN** the section SHALL display a message like "Nothing currently watching"

#### Scenario: Card click navigates to detail
- **WHEN** a user clicks a currently-watching card
- **THEN** the system SHALL navigate to `/watch-spaces/:id/anime/:animeId`

### Requirement: Backlog highlights grid
The dashboard SHALL render a grid of up to 5 anime from the backlog. Each card SHALL display the cover image (or a placeholder), the preferred title, and a "Backlog" badge. Cards SHALL link to the anime detail page.

#### Scenario: Backlog with data
- **WHEN** the dashboard response contains 3 backlog highlights
- **THEN** the grid SHALL render 3 cards with cover images, titles, and backlog badges

#### Scenario: No backlog items
- **WHEN** the `backlogHighlights` array is empty
- **THEN** the section SHALL display a message like "No anime in backlog"

### Requirement: Rating gap highlights list
The dashboard SHALL render up to 3 anime with the largest rating gaps. Each entry SHALL display a small cover thumbnail, the anime title, each rater's display name and score, and the gap delta value. Entries SHALL link to the anime detail page.

#### Scenario: Rating gaps with data
- **WHEN** the dashboard response contains 2 rating-gap highlights with raters and gap values
- **THEN** the list SHALL render 2 entries showing the title, each rater's name and score side by side, and the delta (e.g., "Delta 3.5")

#### Scenario: No rating gaps
- **WHEN** the `ratingGapHighlights` array is empty
- **THEN** the section SHALL display a message like "No rating gaps yet — rate some anime together!"

### Requirement: Dashboard loading state
The dashboard SHALL display skeleton placeholder blocks while the API call is in progress. Skeletons SHALL approximate the layout of each section (stat cards, compatibility, grids, list).

#### Scenario: Initial load
- **WHEN** the dashboard API call is in progress
- **THEN** the page SHALL render skeleton placeholders for stat cards, compatibility, currently watching, backlog, and rating gaps sections

#### Scenario: Load completes
- **WHEN** the API call completes successfully
- **THEN** the skeletons SHALL be replaced with the actual data

### Requirement: Dashboard error state
The dashboard SHALL display an error message with a retry button if the API call fails.

#### Scenario: API failure
- **WHEN** the dashboard API call returns an error (e.g., 500)
- **THEN** the page SHALL display "Failed to load dashboard" with a retry button

#### Scenario: Retry after error
- **WHEN** the user clicks the retry button
- **THEN** the system SHALL re-fetch `GET /watchspaces/{id}/dashboard` and show the loading state

### Requirement: Dashboard header with navigation
The dashboard SHALL display a header with the watch space name, a back link to the watch spaces list, member avatars, and navigation links to the anime list/manage view, add anime, and the analytics page.

#### Scenario: Header rendering
- **WHEN** the dashboard loads successfully
- **THEN** the header SHALL display the watch space name, a "Back to Watch Spaces" link, and buttons for "Anime List", "Add Anime", and "Analytics"

#### Scenario: Navigate to anime list
- **WHEN** the user clicks the "Anime List" link
- **THEN** the system SHALL navigate to `/watch-spaces/:id/manage`

#### Scenario: Navigate to analytics
- **WHEN** the user clicks the "Analytics" link
- **THEN** the system SHALL navigate to `/watch-spaces/:id/analytics`
