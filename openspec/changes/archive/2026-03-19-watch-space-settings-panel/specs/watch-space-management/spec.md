## MODIFIED Requirements

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
