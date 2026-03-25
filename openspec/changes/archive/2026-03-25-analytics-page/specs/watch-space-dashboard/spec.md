## MODIFIED Requirements

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
