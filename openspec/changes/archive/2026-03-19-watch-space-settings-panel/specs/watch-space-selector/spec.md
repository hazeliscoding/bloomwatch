## ADDED Requirements

### Requirement: WatchSpaceService provides detail retrieval

The `WatchSpaceService` SHALL provide a method to fetch a single watch space by ID including its member list.

#### Scenario: Fetch watch space detail

- **WHEN** `getWatchSpaceById(id)` is called
- **THEN** the service SHALL send `GET /watchspaces/{id}` and return an `Observable<WatchSpaceDetail>`

### Requirement: WatchSpaceService provides rename method

The `WatchSpaceService` SHALL provide a method to rename a watch space.

#### Scenario: Rename a watch space

- **WHEN** `renameWatchSpace(id, name)` is called
- **THEN** the service SHALL send `PATCH /watchspaces/{id}` with `{ name }` and return an `Observable`

### Requirement: WatchSpaceService provides member removal method

The `WatchSpaceService` SHALL provide a method to remove a member from a watch space.

#### Scenario: Remove a member

- **WHEN** `removeMember(spaceId, userId)` is called
- **THEN** the service SHALL send `DELETE /watchspaces/{spaceId}/members/{userId}` and return an `Observable`

### Requirement: WatchSpaceService provides ownership transfer method

The `WatchSpaceService` SHALL provide a method to transfer ownership of a watch space.

#### Scenario: Transfer ownership

- **WHEN** `transferOwnership(spaceId, newOwnerId)` is called
- **THEN** the service SHALL send `POST /watchspaces/{spaceId}/transfer-ownership` with `{ newOwnerId }` and return an `Observable`

### Requirement: WatchSpaceService provides leave method

The `WatchSpaceService` SHALL provide a method for the current user to leave a watch space.

#### Scenario: Leave a watch space

- **WHEN** `leaveWatchSpace(spaceId)` is called
- **THEN** the service SHALL send `DELETE /watchspaces/{spaceId}/members/me` and return an `Observable`
