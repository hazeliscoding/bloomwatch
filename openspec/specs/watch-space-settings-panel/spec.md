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

### Requirement: Owner can rename the watch space inline

The system SHALL allow the owner to edit the watch space name via an inline edit control. Non-owners SHALL NOT see the edit control.

#### Scenario: Owner clicks edit to rename

- **WHEN** the owner clicks the edit action next to the space name
- **THEN** the system SHALL display an input field pre-filled with the current name, along with save and cancel actions

#### Scenario: Owner saves a new name

- **WHEN** the owner enters a valid new name and saves
- **THEN** the system SHALL send `PATCH /watchspaces/{id}` with the new name, update the displayed name on success, and exit edit mode

#### Scenario: Owner cancels rename

- **WHEN** the owner clicks cancel during name editing
- **THEN** the system SHALL discard changes and restore the original name display

#### Scenario: Rename API error

- **WHEN** the rename API call fails
- **THEN** the system SHALL display an error message and keep the edit mode open with the entered name preserved

#### Scenario: Non-owner does not see edit control

- **WHEN** a non-owner member views the watch space detail
- **THEN** the system SHALL display the space name as plain text with no edit action

### Requirement: Member list displays all members with details

The system SHALL display a list of all watch space members showing each member's display name, role (as a badge), and join date.

#### Scenario: Member list renders correctly

- **WHEN** the watch space detail is loaded
- **THEN** the system SHALL display each member with their display name, a role badge (color-coded: pink for Owner, blue for Member), and their join date

### Requirement: Owner can remove a non-owner member

The system SHALL allow the owner to remove any non-owner member from the watch space. A confirmation prompt MUST be shown before the removal.

#### Scenario: Owner clicks remove on a member

- **WHEN** the owner clicks the remove action next to a non-owner member and confirms the prompt
- **THEN** the system SHALL send `DELETE /watchspaces/{id}/members/{userId}`, remove the member from the displayed list on success, and show a success indication

#### Scenario: Owner cancels member removal

- **WHEN** the owner clicks remove but cancels the confirmation prompt
- **THEN** the system SHALL take no action

#### Scenario: Remove action not shown for the owner themselves

- **WHEN** the owner views the member list
- **THEN** the system SHALL NOT display a remove action next to the owner's own entry

#### Scenario: Non-owner does not see remove actions

- **WHEN** a non-owner member views the member list
- **THEN** the system SHALL NOT display remove actions next to any member

### Requirement: Owner can transfer ownership

The system SHALL allow the owner to transfer ownership to another member. A confirmation prompt MUST be shown before the transfer.

#### Scenario: Owner transfers ownership

- **WHEN** the owner clicks the transfer ownership action next to a non-owner member and confirms
- **THEN** the system SHALL send `POST /watchspaces/{id}/transfer-ownership` with the target member's ID, update both members' roles in the displayed list, and the current user SHALL see their owner controls removed

#### Scenario: Owner cancels transfer

- **WHEN** the owner clicks transfer but cancels the confirmation prompt
- **THEN** the system SHALL take no action

#### Scenario: Transfer action not visible to non-owners

- **WHEN** a non-owner member views the member list
- **THEN** the system SHALL NOT display transfer ownership actions

### Requirement: Non-owner member can leave the watch space

The system SHALL allow a non-owner member to leave the watch space. After leaving, the user SHALL be redirected to the watch space list.

#### Scenario: Member leaves the watch space

- **WHEN** a non-owner member clicks "Leave Space" and confirms
- **THEN** the system SHALL send `DELETE /watchspaces/{id}/members/me`, and redirect the user to `/watch-spaces`

#### Scenario: Leave action not shown to owners

- **WHEN** the owner views the settings panel
- **THEN** the system SHALL NOT display a "Leave Space" action (owners must transfer ownership first)
