## ADDED Requirements

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

---

### Requirement: Invited user can accept an invitation via token
The system SHALL allow an authenticated user to accept a `Pending`, non-expired invitation addressed to their registered email. On acceptance the user SHALL become a `Member` of the watch space and the invitation status SHALL change to `Accepted`.

#### Scenario: Successful acceptance
- **WHEN** an authenticated user sends `POST /watchspaces/invitations/{token}/accept` and the token matches a `Pending` invitation addressed to their email that has not expired
- **THEN** the system SHALL return HTTP 200, the user SHALL be added as a `Member` of the watch space, and the invitation `status` SHALL be `Accepted`

#### Scenario: Expired invitation rejected
- **WHEN** an authenticated user sends `POST /watchspaces/invitations/{token}/accept` and the invitation's `expiresAt` is in the past
- **THEN** the system SHALL return HTTP 410 Gone

#### Scenario: Token not addressed to caller rejected
- **WHEN** an authenticated user sends `POST /watchspaces/invitations/{token}/accept` but the invitation was sent to a different email address
- **THEN** the system SHALL return HTTP 403 Forbidden

#### Scenario: Non-existent token rejected
- **WHEN** an authenticated user sends `POST /watchspaces/invitations/{token}/accept` with a token that does not exist
- **THEN** the system SHALL return HTTP 404 Not Found

#### Scenario: Already-accepted invitation rejected
- **WHEN** an authenticated user sends `POST /watchspaces/invitations/{token}/accept` for an invitation with status `Accepted` or `Declined`
- **THEN** the system SHALL return HTTP 409 Conflict

---

### Requirement: Invited user can decline an invitation via token
The system SHALL allow an authenticated user to decline a `Pending`, non-expired invitation addressed to their email. The invitation status SHALL change to `Declined` and the user SHALL NOT be added to the space.

#### Scenario: Successful decline
- **WHEN** an authenticated user sends `POST /watchspaces/invitations/{token}/decline` and the token matches a `Pending` invitation addressed to their email
- **THEN** the system SHALL return HTTP 200 and the invitation `status` SHALL be `Declined`

#### Scenario: Expired invitation can still be declined
- **WHEN** an authenticated user sends `POST /watchspaces/invitations/{token}/decline` for an expired `Pending` invitation addressed to them
- **THEN** the system SHALL return HTTP 200 and update the status to `Declined`

---

### Requirement: Owner can revoke a pending invitation
The system SHALL allow an `Owner` to cancel a `Pending` invitation before it is accepted, setting its status to `Revoked`.

#### Scenario: Successful revocation
- **WHEN** an authenticated `Owner` sends `DELETE /watchspaces/{id}/invitations/{invitationId}` for a `Pending` invitation
- **THEN** the system SHALL return HTTP 200 and the invitation `status` SHALL be `Revoked`

#### Scenario: Cannot revoke accepted invitation
- **WHEN** an authenticated `Owner` sends a revoke request for an invitation with status `Accepted`
- **THEN** the system SHALL return HTTP 409 Conflict

#### Scenario: Non-owner revoke attempt rejected
- **WHEN** an authenticated `Member` sends `DELETE /watchspaces/{id}/invitations/{invitationId}`
- **THEN** the system SHALL return HTTP 403 Forbidden

---

### Requirement: Owner can list invitations for a watch space
The system SHALL allow an `Owner` to retrieve all invitations (any status) for a watch space.

#### Scenario: Successful listing
- **WHEN** an authenticated `Owner` sends `GET /watchspaces/{id}/invitations`
- **THEN** the system SHALL return HTTP 200 with an array of invitations, each including `id`, `invitedEmail`, `status`, `expiresAt`, and `createdAt`

#### Scenario: Non-owner listing rejected
- **WHEN** an authenticated `Member` sends `GET /watchspaces/{id}/invitations`
- **THEN** the system SHALL return HTTP 403 Forbidden
