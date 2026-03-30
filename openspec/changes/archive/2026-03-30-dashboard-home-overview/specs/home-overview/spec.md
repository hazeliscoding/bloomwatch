## ADDED Requirements

### Requirement: Home overview API endpoint
The system SHALL expose `GET /home/overview` as an authenticated endpoint that returns a composite JSON object containing the user's display name, aggregated stats across all watch spaces, a list of watch space summaries with per-space quick stats, and recent anime activity.

#### Scenario: Successful home overview load for a user with watch spaces
- **WHEN** an authenticated user sends `GET /home/overview` and the user belongs to 2 watch spaces containing anime in various statuses
- **THEN** the system SHALL return `200 OK` with a JSON object containing `displayName`, `stats`, `watchSpaces`, and `recentActivity` fields

#### Scenario: Home overview for a user with no watch spaces
- **WHEN** an authenticated user sends `GET /home/overview` and the user belongs to no watch spaces
- **THEN** the system SHALL return `200 OK` with `displayName` populated, `stats` showing all zeroes, an empty `watchSpaces` array, and an empty `recentActivity` array

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated request is sent to `GET /home/overview`
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Home overview stats aggregation
The `stats` object SHALL contain: `watchSpaceCount` (number of watch spaces the user belongs to), `totalAnimeTracked` (sum of all anime across all the user's watch spaces, deduplicated by AniList ID), and `totalEpisodesWatchedTogether` (sum of `sharedEpisodesWatched` across all anime in all the user's watch spaces).

#### Scenario: Stats correctly aggregate across multiple watch spaces
- **WHEN** a user belongs to watch space A with 10 anime (80 episodes together) and watch space B with 5 anime (40 episodes together), with 2 anime appearing in both spaces
- **THEN** `stats.watchSpaceCount` SHALL be 2, `stats.totalAnimeTracked` SHALL be 13 (deduplicated), and `stats.totalEpisodesWatchedTogether` SHALL be 120

#### Scenario: Stats with single watch space
- **WHEN** a user belongs to 1 watch space with 7 anime and 52 episodes watched together
- **THEN** `stats.watchSpaceCount` SHALL be 1, `stats.totalAnimeTracked` SHALL be 7, and `stats.totalEpisodesWatchedTogether` SHALL be 52

#### Scenario: Stats with no watch spaces
- **WHEN** a user belongs to no watch spaces
- **THEN** `stats.watchSpaceCount` SHALL be 0, `stats.totalAnimeTracked` SHALL be 0, and `stats.totalEpisodesWatchedTogether` SHALL be 0

### Requirement: Home overview watch space summaries
The `watchSpaces` array SHALL contain one entry per watch space the user belongs to, ordered by most recently created first. Each entry SHALL include `watchSpaceId`, `name`, `role`, `memberCount`, `memberPreviews` (array of `{ displayName }` for up to 5 members), `watchingCount` (number of anime with `sharedStatus = Watching`), and `backlogCount` (number of anime with `sharedStatus = Backlog`).

#### Scenario: Watch space summaries with data
- **WHEN** a user belongs to 2 watch spaces, the first with 3 watching and 5 backlog anime, the second with 1 watching and 10 backlog anime
- **THEN** `watchSpaces` SHALL contain 2 entries with `watchingCount` and `backlogCount` reflecting those values

#### Scenario: Watch space with no anime
- **WHEN** a user belongs to a watch space that contains no anime
- **THEN** the corresponding entry SHALL have `watchingCount` of 0 and `backlogCount` of 0

#### Scenario: Member previews limited to 5
- **WHEN** a watch space has 8 members
- **THEN** `memberPreviews` SHALL contain exactly 5 entries and `memberCount` SHALL be 8

### Requirement: Home overview recent activity
The `recentActivity` array SHALL contain up to 3 anime most recently updated across all the user's watch spaces, ordered by `lastUpdatedAt` descending. Each entry SHALL include `watchSpaceAnimeId`, `watchSpaceId`, `watchSpaceName`, `preferredTitle`, `coverImageUrl`, `sharedStatus`, and `lastUpdatedAt`.

#### Scenario: Recent activity across multiple watch spaces
- **WHEN** a user has watch space A with an anime updated at 18:00 and watch space B with an anime updated at 19:00 and another at 17:00
- **THEN** `recentActivity` SHALL contain 3 entries ordered by `lastUpdatedAt` descending: the 19:00 entry first, 18:00 second, 17:00 third

#### Scenario: Fewer than 3 recently updated anime
- **WHEN** a user has only 1 anime across all watch spaces
- **THEN** `recentActivity` SHALL contain exactly 1 entry

#### Scenario: No anime in any watch space
- **WHEN** a user has watch spaces but none contain anime
- **THEN** `recentActivity` SHALL be an empty array

#### Scenario: More than 3 recently updated anime
- **WHEN** a user has 10 anime across all watch spaces
- **THEN** `recentActivity` SHALL contain exactly 3 entries, the 3 most recently updated

### Requirement: Home page route
The application SHALL render the Home page component at the `/` route within the authenticated shell layout. The Home page SHALL be a standalone Angular component that is lazy-loaded via `loadChildren`. The shell navigation bar SHALL display "Home" (not "Dashboard") for this route.

#### Scenario: Authenticated user navigates to root
- **WHEN** an authenticated user navigates to `/`
- **THEN** the Home page component SHALL render inside the shell layout with the navigation bar visible

#### Scenario: Nav bar shows Home label
- **WHEN** the shell navigation bar renders
- **THEN** the first nav link SHALL display "Home" and be active when the route is `/`

### Requirement: Home page greeting section
The Home page SHALL display a greeting section with the text "Welcome back, {displayName}" using the display font family and gradient text treatment. The display name SHALL come from the `GET /home/overview` response.

#### Scenario: Greeting with display name
- **WHEN** the home overview API returns `displayName: "Hazel"`
- **THEN** the greeting heading SHALL read "Welcome back, Hazel"

#### Scenario: Greeting during loading
- **WHEN** the home overview API call is in progress
- **THEN** the greeting section SHALL display a skeleton placeholder

### Requirement: Home page global stats display
The Home page SHALL render a row of 3 stat cards showing: Watch Spaces count, Anime Tracked count, and Episodes Together count, using the values from `stats` in the home overview response.

#### Scenario: Stats with data
- **WHEN** the home overview response contains `stats.watchSpaceCount = 3`, `stats.totalAnimeTracked = 42`, `stats.totalEpisodesWatchedTogether = 584`
- **THEN** the stat cards SHALL display "3" for Watch Spaces, "42" for Anime Tracked, and "584" for Episodes Together

#### Scenario: Stats with all zeroes
- **WHEN** the home overview response contains all zero stats
- **THEN** the stat cards SHALL display "0" for each value

### Requirement: Home page watch space cards
The Home page SHALL render a responsive card grid showing the user's watch spaces. Each card SHALL display the watch space name, the user's role badge, member avatars (using `bloom-avatar-stack`), watching count, backlog count, and a button or link navigating to `/watch-spaces/{id}`.

#### Scenario: Multiple watch spaces displayed
- **WHEN** the home overview response contains 3 watch spaces
- **THEN** the grid SHALL render 3 cards with names, role badges, member avatars, and stat counts

#### Scenario: Watch space card navigation
- **WHEN** a user clicks or activates a watch space card's action
- **THEN** the system SHALL navigate to `/watch-spaces/{watchSpaceId}`

#### Scenario: Single watch space
- **WHEN** the user has exactly 1 watch space
- **THEN** the grid SHALL render 1 card

### Requirement: Home page recent activity section
The Home page SHALL render a "Recent Activity" section showing up to 3 recently updated anime. Each entry SHALL display the cover image (or placeholder), the preferred title, the watch space name as context, a status badge (`bloom-badge`), and the relative time since last update. Each entry SHALL link to the anime detail page at `/watch-spaces/{watchSpaceId}/anime/{watchSpaceAnimeId}`.

#### Scenario: Recent activity with data
- **WHEN** the home overview response contains 3 recent activity entries
- **THEN** the section SHALL render 3 items with cover images, titles, watch space names, status badges, and relative timestamps

#### Scenario: No recent activity
- **WHEN** the `recentActivity` array is empty
- **THEN** the section SHALL display a message such as "No recent activity yet"

#### Scenario: Activity item navigation
- **WHEN** a user clicks a recent activity item
- **THEN** the system SHALL navigate to `/watch-spaces/{watchSpaceId}/anime/{watchSpaceAnimeId}`

### Requirement: Home page quick actions
The Home page SHALL display quick-action buttons: a primary "Create Watch Space" button and a secondary "Browse Anime" button. "Create Watch Space" SHALL navigate to the watch space creation flow. "Browse Anime" SHALL navigate to the anime search functionality.

#### Scenario: Create Watch Space action
- **WHEN** a user clicks "Create Watch Space"
- **THEN** the system SHALL navigate to the watch space creation route

#### Scenario: Browse Anime action
- **WHEN** a user clicks "Browse Anime"
- **THEN** the system SHALL navigate to the anime search route

### Requirement: Home page empty state
When the authenticated user has no watch spaces, the Home page SHALL replace the stats strip, watch space grid, and recent activity section with a single onboarding card. The card SHALL display a friendly heading (e.g., "Your anime journey starts here"), a short description, and a primary "Create Your First Watch Space" CTA button.

#### Scenario: New user with no watch spaces
- **WHEN** the home overview response returns an empty `watchSpaces` array and zero stats
- **THEN** the page SHALL display the onboarding empty state card with a CTA instead of stats, grid, and activity sections

#### Scenario: User creates first watch space then returns
- **WHEN** the user returns to the Home page after creating a watch space
- **THEN** the page SHALL display the normal stats, watch space card, and activity sections

### Requirement: Home page loading state
The Home page SHALL display skeleton placeholder blocks while the `GET /home/overview` API call is in progress. Skeletons SHALL approximate the layout of each section (greeting, stats, watch space grid, recent activity).

#### Scenario: Initial load
- **WHEN** the home overview API call is in progress
- **THEN** the page SHALL render skeleton placeholders for the greeting, stat cards, watch space grid, and recent activity sections

#### Scenario: Load completes
- **WHEN** the API call completes successfully
- **THEN** the skeletons SHALL be replaced with the actual data

### Requirement: Home page error state
The Home page SHALL display an error message with a retry button if the `GET /home/overview` API call fails.

#### Scenario: API failure
- **WHEN** the home overview API call returns an error
- **THEN** the page SHALL display "Something went wrong" with a retry button

#### Scenario: Retry after error
- **WHEN** the user clicks the retry button
- **THEN** the system SHALL re-fetch `GET /home/overview` and show the loading state

### Requirement: Design system compliance
The Home page SHALL use only semantic design tokens from the BloomWatch design system. It SHALL use `bloom-` prefixed shared components and follow BEM naming conventions for its own SCSS classes. All text and interactive elements SHALL meet WCAG 2.1 AA contrast and accessibility requirements.

#### Scenario: Component uses design tokens
- **WHEN** a developer inspects the Home page SCSS
- **THEN** all spacing, color, font-size, and layout values SHALL reference `--bloom-*` custom properties

#### Scenario: Accessibility compliance
- **WHEN** a screen reader user navigates the Home page
- **THEN** all interactive elements SHALL be keyboard accessible, stat cards SHALL use semantic headings, and images SHALL have appropriate alt text
