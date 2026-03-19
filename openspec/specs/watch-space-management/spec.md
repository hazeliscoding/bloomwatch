## ADDED Requirements

### Requirement: Authenticated user can create a watch space
The system SHALL allow an authenticated user to create a new watch space by providing a name (1–100 characters). On creation the creator SHALL automatically become a member with the `Owner` role.

#### Scenario: Successful creation
- **WHEN** an authenticated user sends `POST /watchspaces` with a valid `name`
- **THEN** the system SHALL return HTTP 201 with the new watch space's `id`, `name`, `createdAt`, and the caller's membership role (`Owner`)

#### Scenario: Name too long rejected
- **WHEN** an authenticated user sends `POST /watchspaces` with a `name` exceeding 100 characters
- **THEN** the system SHALL return HTTP 400 with a validation error

#### Scenario: Empty name rejected
- **WHEN** an authenticated user sends `POST /watchspaces` with an empty or whitespace-only `name`
- **THEN** the system SHALL return HTTP 400 with a validation error

#### Scenario: Unauthenticated request rejected
- **WHEN** a request is sent to `POST /watchspaces` without a valid JWT
- **THEN** the system SHALL return HTTP 401

---

### Requirement: Owner can rename a watch space
The system SHALL allow a member with the `Owner` role to rename a watch space.

#### Scenario: Successful rename
- **WHEN** an authenticated `Owner` sends `PATCH /watchspaces/{id}` with a valid new `name`
- **THEN** the system SHALL return HTTP 200 with the updated watch space name

#### Scenario: Non-owner rename rejected
- **WHEN** an authenticated `Member` (non-owner) sends `PATCH /watchspaces/{id}` with a new `name`
- **THEN** the system SHALL return HTTP 403 Forbidden

#### Scenario: Rename on non-existent space
- **WHEN** an authenticated user sends `PATCH /watchspaces/{id}` for an ID that does not exist
- **THEN** the system SHALL return HTTP 404 Not Found

---

### Requirement: Authenticated user can retrieve their watch spaces
The system SHALL allow an authenticated user to list all watch spaces they are a member of, regardless of role.

#### Scenario: List returns only member's spaces
- **WHEN** an authenticated user sends `GET /watchspaces`
- **THEN** the system SHALL return HTTP 200 with an array containing only the watch spaces where the caller is an active member

#### Scenario: Empty list when user has no spaces
- **WHEN** an authenticated user with no memberships sends `GET /watchspaces`
- **THEN** the system SHALL return HTTP 200 with an empty array

---

### Requirement: Member can retrieve a single watch space by ID
The system SHALL allow a member of a watch space to retrieve its details, including the member list with display names.

#### Scenario: Successful retrieval
- **WHEN** an authenticated member sends `GET /watchspaces/{id}`
- **THEN** the system SHALL return HTTP 200 with the space's `id`, `name`, `createdAt`, and a list of members (each with `userId`, `displayName`, `role`, `joinedAt`)

#### Scenario: Non-member access rejected
- **WHEN** an authenticated user who is not a member sends `GET /watchspaces/{id}`
- **THEN** the system SHALL return HTTP 403 Forbidden

#### Scenario: Non-existent space returns 404
- **WHEN** an authenticated user sends `GET /watchspaces/{id}` for an ID that does not exist
- **THEN** the system SHALL return HTTP 404 Not Found

---

### Requirement: Owner can transfer ownership to another member
The system SHALL allow an `Owner` to transfer their ownership role to an existing `Member` of the same watch space. After transfer the previous owner becomes a `Member`.

#### Scenario: Successful ownership transfer
- **WHEN** an authenticated `Owner` sends `POST /watchspaces/{id}/transfer-ownership` with a valid `newOwnerId` who is a member
- **THEN** the system SHALL return HTTP 200, the specified user SHALL have role `Owner`, and the previous owner SHALL have role `Member`

#### Scenario: Transfer to non-member rejected
- **WHEN** an authenticated `Owner` sends a transfer request for a user who is not a member of the space
- **THEN** the system SHALL return HTTP 400

#### Scenario: Non-owner transfer attempt rejected
- **WHEN** an authenticated `Member` sends `POST /watchspaces/{id}/transfer-ownership`
- **THEN** the system SHALL return HTTP 403 Forbidden
