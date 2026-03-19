### Requirement: Owner can view pending invitations list
The system SHALL display a list of pending invitations within the invitations section of the settings panel, visible only to the owner. The list SHALL be fetched from `GET /watchspaces/{id}/invitations`.

#### Scenario: Owner sees pending invitations
- **WHEN** an authenticated owner views the watch space detail page and there are pending invitations
- **THEN** the system SHALL display each pending invitation showing the invited email, sent date, and expiry date

#### Scenario: No pending invitations
- **WHEN** an authenticated owner views the watch space detail page and there are no pending invitations
- **THEN** the system SHALL display a message indicating there are no pending invitations

#### Scenario: Invitations list loads when the page loads
- **WHEN** the watch space detail page loads for an owner
- **THEN** the system SHALL fetch the invitations list from `GET /watchspaces/{id}/invitations` and display them

#### Scenario: Non-owner does not see invitations list
- **WHEN** a non-owner member views the watch space detail page
- **THEN** the system SHALL NOT display the invitations list or fetch invitation data

### Requirement: Owner can revoke a pending invitation
The system SHALL allow the owner to revoke any pending invitation with a confirmation step. Revoking SHALL call `DELETE /watchspaces/{id}/invitations/{invitationId}`.

#### Scenario: Owner clicks revoke on a pending invitation
- **WHEN** the owner clicks the revoke action on a pending invitation and confirms the prompt
- **THEN** the system SHALL send `DELETE /watchspaces/{id}/invitations/{invitationId}` and remove the invitation from the displayed list on success

#### Scenario: Owner cancels revocation
- **WHEN** the owner clicks the revoke action but cancels the confirmation prompt
- **THEN** the system SHALL take no action and the invitation SHALL remain in the list

#### Scenario: Revoke of already-processed invitation fails
- **WHEN** the owner attempts to revoke an invitation that has already been accepted or declined
- **THEN** the system SHALL display an inline error message indicating the invitation cannot be revoked (HTTP 409)

#### Scenario: Revoke API error
- **WHEN** the revoke API call fails for any other reason
- **THEN** the system SHALL display an error message and the invitation SHALL remain in the list
