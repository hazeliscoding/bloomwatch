## ADDED Requirements

### Requirement: Invitee can view an invitation accept/decline page
The system SHALL provide a page at `/watch-spaces/invitations/:token` that loads the invitation context and presents the invitee with accept and decline actions.

#### Scenario: Invitee navigates to a valid invitation link
- **WHEN** an authenticated user navigates to `/watch-spaces/invitations/:token` with a valid, pending, non-expired token addressed to their email
- **THEN** the system SHALL display the watch space name and present "Accept" and "Decline" buttons

#### Scenario: Loading state while resolving token
- **WHEN** the invitation page is loading
- **THEN** the system SHALL display a loading indicator

### Requirement: Invitee can accept an invitation
The system SHALL allow the invitee to accept a pending invitation, adding them as a member and redirecting to the watch space.

#### Scenario: Successful acceptance
- **WHEN** the invitee clicks "Accept" on a valid pending invitation
- **THEN** the system SHALL send `POST /watchspaces/invitations/{token}/accept`, and on success redirect the user to `/watch-spaces/{watchSpaceId}`

#### Scenario: Accept button shows loading state
- **WHEN** the invitee clicks "Accept" and the API call is in progress
- **THEN** the system SHALL disable both buttons and show a loading state on the "Accept" button

### Requirement: Invitee can decline an invitation
The system SHALL allow the invitee to decline a pending invitation and see a confirmation.

#### Scenario: Successful decline
- **WHEN** the invitee clicks "Decline" on a valid pending invitation
- **THEN** the system SHALL send `POST /watchspaces/invitations/{token}/decline`, and on success display a "Declined" confirmation message

#### Scenario: Decline button shows loading state
- **WHEN** the invitee clicks "Decline" and the API call is in progress
- **THEN** the system SHALL disable both buttons and show a loading state on the "Decline" button

### Requirement: Invitation error states show user-friendly messages
The system SHALL display clear error messages for invalid, expired, or already-processed invitation tokens instead of raw HTTP errors.

#### Scenario: Expired token
- **WHEN** an authenticated user navigates to an invitation link with an expired token
- **THEN** the system SHALL display a message indicating the invitation has expired (HTTP 410)

#### Scenario: Token not addressed to the current user
- **WHEN** an authenticated user navigates to an invitation link addressed to a different email
- **THEN** the system SHALL display a message indicating the invitation is for a different account (HTTP 403)

#### Scenario: Non-existent token
- **WHEN** an authenticated user navigates to an invitation link with a token that does not exist
- **THEN** the system SHALL display a message indicating the invitation was not found (HTTP 404)

#### Scenario: Already-processed invitation
- **WHEN** an authenticated user navigates to an invitation link that has already been accepted or declined
- **THEN** the system SHALL display a message indicating the invitation has already been used (HTTP 409)
