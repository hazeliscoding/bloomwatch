## MODIFIED Requirements

### Requirement: Owner can invite a user to a watch space by email
The system SHALL allow a member with the `Owner` role to invite another user by their registered email address. The system SHALL create an `Invitation` record with a unique opaque token and an expiry of 7 days from creation. After persisting the invitation, the system SHALL attempt to send a notification email to the invitee (with retry); failure to deliver the email SHALL NOT prevent the invitation from being created or the 201 response from being returned, but SHALL be indicated in the response body via `emailDeliveryFailed: true`.

#### Scenario: Successful invitation to a registered user
- **WHEN** an authenticated `Owner` sends `POST /watchspaces/{id}/invitations` with an `email` that matches a registered BloomWatch account
- **THEN** the system SHALL return HTTP 201 with the invitation `id`, `invitedEmail`, `status` (`Pending`), `expiresAt`, and `emailDeliveryFailed: false`

#### Scenario: Invitation email is sent on success
- **WHEN** an authenticated `Owner` successfully creates an invitation
- **THEN** an email SHALL be sent to the `invitedEmail` address containing the inviter's display name, the watch space name, and accept/decline links

#### Scenario: Email delivery failure is surfaced to the owner
- **WHEN** an authenticated `Owner` successfully creates an invitation but all email delivery attempts fail
- **THEN** the system SHALL return HTTP 201 with the invitation data AND `emailDeliveryFailed: true`; the failure SHALL be logged at Error level

#### Scenario: Invitation to an already-active member rejected
- **WHEN** an authenticated `Owner` sends an invitation to an `email` already belonging to an active member of the space
- **THEN** the system SHALL return HTTP 409 Conflict

#### Scenario: Duplicate pending invitation rejected
- **WHEN** an authenticated `Owner` sends an invitation to an `email` that already has a `Pending` invitation for the same space
- **THEN** the system SHALL return HTTP 409 Conflict

#### Scenario: Non-owner invite attempt rejected
- **WHEN** an authenticated `Member` (non-owner) sends `POST /watchspaces/{id}/invitations`
- **THEN** the system SHALL return HTTP 403 Forbidden

#### Scenario: Invitation to unregistered email rejected
- **WHEN** an authenticated `Owner` sends an invitation to an `email` that does not match any registered BloomWatch account
- **THEN** the system SHALL return HTTP 422 with an error indicating the email is not a registered user
