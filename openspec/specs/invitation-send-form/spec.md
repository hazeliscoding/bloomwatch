### Requirement: Owner can send an invitation from the settings panel
The system SHALL display an invite form within the watch space settings panel, visible only to the owner. The form SHALL accept an email address and submit to `POST /watchspaces/{id}/invitations`.

#### Scenario: Owner sees the invite form
- **WHEN** an authenticated owner views the watch space detail page
- **THEN** the system SHALL display an email input field and an "Invite" submit button in the invitations section of the settings panel

#### Scenario: Non-owner does not see the invite form
- **WHEN** an authenticated non-owner member views the watch space detail page
- **THEN** the system SHALL NOT display the invite form or the invitations section

#### Scenario: Successful invitation send
- **WHEN** the owner enters a valid email address and clicks "Invite"
- **THEN** the system SHALL send `POST /watchspaces/{id}/invitations` with the email, display a success confirmation message, clear the email input, and refresh the pending invitations list

#### Scenario: Invite button is disabled while submitting
- **WHEN** the owner clicks "Invite" and the API call is in progress
- **THEN** the system SHALL disable the invite button and show a loading state until the call completes

#### Scenario: Already a member error
- **WHEN** the owner sends an invitation to an email that belongs to an existing member of the space
- **THEN** the system SHALL display an inline error message indicating the user is already a member (HTTP 409)

#### Scenario: Duplicate pending invitation error
- **WHEN** the owner sends an invitation to an email that already has a pending invitation for this space
- **THEN** the system SHALL display an inline error message indicating a pending invitation already exists (HTTP 409)

#### Scenario: Unregistered email error
- **WHEN** the owner sends an invitation to an email not associated with any registered BloomWatch account
- **THEN** the system SHALL display an inline error message indicating the email is not a registered user (HTTP 422)

#### Scenario: Empty email validation
- **WHEN** the owner clicks "Invite" with an empty or whitespace-only email field
- **THEN** the system SHALL NOT submit the request and SHALL show a validation message
