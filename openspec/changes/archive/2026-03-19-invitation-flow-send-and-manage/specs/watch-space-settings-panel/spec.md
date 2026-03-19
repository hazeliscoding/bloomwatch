## MODIFIED Requirements

### Requirement: Settings panel is accessible from the watch space detail view

The system SHALL display a settings panel within the watch space detail page at `/watch-spaces/:id`. The panel SHALL be visible to all members of the watch space. For owners, the panel SHALL additionally include an invitations section containing the invite form and pending invitations list.

#### Scenario: Member navigates to watch space detail
- **WHEN** an authenticated member navigates to `/watch-spaces/:id`
- **THEN** the system SHALL load the watch space detail (including members) and display the settings panel with the space name and member list

#### Scenario: Owner sees the invitations section
- **WHEN** an authenticated owner navigates to `/watch-spaces/:id`
- **THEN** the system SHALL display the invitations section below the members section, containing the invite form and the pending invitations list

#### Scenario: Non-owner does not see the invitations section
- **WHEN** a non-owner member navigates to `/watch-spaces/:id`
- **THEN** the system SHALL NOT display the invitations section

#### Scenario: Loading state while fetching detail
- **WHEN** the watch space detail is being loaded from the API
- **THEN** the system SHALL display a loading indicator

#### Scenario: API error while loading detail
- **WHEN** the `GET /watchspaces/{id}` request fails
- **THEN** the system SHALL display an error message
