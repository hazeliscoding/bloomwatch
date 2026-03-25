### Requirement: Analytics page loads data from three endpoints in parallel
The system SHALL render an analytics page at `/watch-spaces/:id/analytics` that fetches `GET /watchspaces/{id}/analytics/compatibility`, `GET /watchspaces/{id}/analytics/rating-gaps`, and `GET /watchspaces/{id}/analytics/shared-stats` in parallel on load.

#### Scenario: Successful load with full data
- **WHEN** a member navigates to `/watch-spaces/:id/analytics` and all three APIs return data
- **THEN** the page SHALL render the compatibility section, shared stats section, rating gaps list, and rating comparison chart

#### Scenario: One endpoint fails
- **WHEN** one of the three API calls fails while the others succeed
- **THEN** the failed section SHALL display an error message with a retry button while the other sections render normally

### Requirement: Compatibility section with breakdown stats
The analytics page SHALL render a compatibility section containing the `bloom-compat-ring` component and a breakdown panel showing Average Gap, Anime Rated Together count, and the formula used.

#### Scenario: Compatibility data available
- **WHEN** the compatibility endpoint returns a non-null compatibility object with `score = 87`, `averageGap = 1.3`, `ratedTogetherCount = 9`
- **THEN** the section SHALL render the compatibility ring with score 87, and a breakdown panel showing "1.3 points" for Average Gap, "9" for Anime Rated Together

#### Scenario: Compatibility is null (insufficient data)
- **WHEN** the compatibility endpoint returns `compatibility = null` with a message
- **THEN** the section SHALL render the `bloom-compat-ring` placeholder state and SHALL NOT render the breakdown panel

### Requirement: Shared stats section
The analytics page SHALL render a shared stats section with a 2x2 grid of stat items and a most-recent-session date row.

#### Scenario: Stats with data
- **WHEN** the shared stats endpoint returns `totalEpisodesWatchedTogether = 184`, `totalFinished = 11`, `totalDropped = 1`, `totalWatchSessions = 23`, `mostRecentSessionDate = "2026-03-15"`
- **THEN** the section SHALL display stat items for "184 Episodes together", "11 Shows finished", "1 Shows dropped", "23 Watch sessions", and "Most Recent Session: Mar 15, 2026"

#### Scenario: No sessions recorded
- **WHEN** the shared stats endpoint returns all zeroes and `mostRecentSessionDate = null`
- **THEN** the section SHALL display "0" for all stat items and "No sessions yet" for the most recent session row

### Requirement: Full rating gaps list with score bars
The analytics page SHALL render a list of all anime with rating gaps, showing each entry with a cover thumbnail, title, per-member horizontal score bars, numeric scores, and gap delta. Items SHALL be displayed in the order returned by the API (descending gap).

#### Scenario: Multiple rating gaps
- **WHEN** the rating gaps endpoint returns 4 items
- **THEN** the list SHALL render 4 rows, each showing the anime title, a horizontal bar per rater scaled proportionally to their score out of 10, the numeric scores, and the gap delta

#### Scenario: No rating gaps
- **WHEN** the rating gaps endpoint returns an empty items array with a message
- **THEN** the section SHALL display the message from the API response

### Requirement: Rating comparison grouped bar chart
The analytics page SHALL render a grouped bar chart (using Chart.js via ng2-charts) comparing member ratings for all anime in the rating gaps list. Each anime SHALL have one bar per member, color-coded by member.

#### Scenario: Chart renders with data
- **WHEN** the rating gaps endpoint returns items with ratings from 2 members
- **THEN** the chart SHALL render a grouped bar chart with anime titles on the X-axis, rating scores (0-10) on the Y-axis, and one color-coded bar per member per anime, with a legend identifying each member

#### Scenario: No data for chart
- **WHEN** the rating gaps endpoint returns an empty items array
- **THEN** the chart section SHALL NOT be rendered

### Requirement: Analytics page loading state
The analytics page SHALL display skeleton placeholder blocks while API calls are in progress. Each section SHALL show its own skeleton independently.

#### Scenario: Initial load
- **WHEN** the analytics page is loading data
- **THEN** the page SHALL render skeleton placeholders for the compatibility section, shared stats section, and rating gaps section

### Requirement: Analytics page header with back navigation
The analytics page SHALL display a header with the watch space name, a back link to the dashboard, and the page title "Analytics".

#### Scenario: Header rendering
- **WHEN** the analytics page loads
- **THEN** the page SHALL display a "Back to Dashboard" link navigating to `/watch-spaces/:id` and the title "Analytics"

### Requirement: Analytics page responsive layout
The analytics page SHALL use a two-column layout for the compatibility and shared stats sections on desktop, collapsing to a single column on mobile.

#### Scenario: Desktop layout
- **WHEN** the viewport is wider than 768px
- **THEN** the compatibility and shared stats sections SHALL render side by side in two columns

#### Scenario: Mobile layout
- **WHEN** the viewport is 768px or narrower
- **THEN** all sections SHALL stack in a single column
