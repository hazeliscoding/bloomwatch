## ADDED Requirements

### Requirement: Owner can remove a member from a watch space
The system SHALL allow a member with the `Owner` role to remove any non-owner member from the watch space. Removing the last `Owner` SHALL NOT be permitted.

#### Scenario: Successful member removal
- **WHEN** an authenticated `Owner` sends `DELETE /watchspaces/{id}/members/{userId}` for a `Member` (non-owner)
- **THEN** the system SHALL return HTTP 200 and the user SHALL no longer appear in the membership list

#### Scenario: Owner cannot remove themselves if they are the last owner
- **WHEN** an authenticated `Owner` sends `DELETE /watchspaces/{id}/members/{ownerId}` where they are the sole owner
- **THEN** the system SHALL return HTTP 409 Conflict with an error stating at least one owner must remain

#### Scenario: Owner cannot remove another owner directly
- **WHEN** an authenticated `Owner` sends `DELETE /watchspaces/{id}/members/{userId}` for another `Owner`
- **THEN** the system SHALL return HTTP 409 Conflict — ownership must be transferred first

#### Scenario: Non-owner removal attempt rejected
- **WHEN** an authenticated `Member` sends `DELETE /watchspaces/{id}/members/{userId}`
- **THEN** the system SHALL return HTTP 403 Forbidden

#### Scenario: Remove non-member rejected
- **WHEN** an authenticated `Owner` sends `DELETE /watchspaces/{id}/members/{userId}` for a user who is not a member
- **THEN** the system SHALL return HTTP 404 Not Found

---

### Requirement: Member can leave a watch space voluntarily
The system SHALL allow any member to remove themselves from a watch space. If the leaving member is the sole `Owner`, the operation SHALL be rejected.

#### Scenario: Successful self-removal as Member
- **WHEN** an authenticated `Member` sends `DELETE /watchspaces/{id}/members/me`
- **THEN** the system SHALL return HTTP 200 and the user SHALL no longer appear in the membership list

#### Scenario: Sole owner cannot leave
- **WHEN** an authenticated `Owner` sends `DELETE /watchspaces/{id}/members/me` and they are the only owner
- **THEN** the system SHALL return HTTP 409 Conflict with an error stating ownership must be transferred before leaving

#### Scenario: Owner with co-owner can leave
- **WHEN** an authenticated `Owner` sends `DELETE /watchspaces/{id}/members/me` and at least one other `Owner` exists in the space
- **THEN** the system SHALL return HTTP 200 and the user SHALL no longer appear in the membership list

---

### Requirement: Watch space enforces the at-least-one-owner invariant
The system SHALL ensure that every watch space always has at least one member with the `Owner` role. This invariant SHALL be enforced in the domain layer and cannot be bypassed through any API operation.

#### Scenario: Invariant enforced on remove
- **WHEN** any operation would leave a watch space with zero `Owner` members
- **THEN** the system SHALL reject the operation before persisting and SHALL return an error indicating the invariant violation

#### Scenario: Newly created space satisfies the invariant
- **WHEN** a watch space is created
- **THEN** the creator SHALL automatically be assigned the `Owner` role, satisfying the invariant from the moment of creation

---

### Requirement: Integration events are published when membership changes
The system SHALL publish integration events after a member joins or leaves a watch space, so downstream modules can react without querying the `watch_spaces` schema.

#### Scenario: MemberJoinedWatchSpace published on invitation acceptance
- **WHEN** a user successfully accepts an invitation
- **THEN** the system SHALL publish a `MemberJoinedWatchSpace` event containing `WatchSpaceId`, `UserId`, `Role`, and `JoinedAtUtc`

#### Scenario: MemberLeftWatchSpace published on removal or voluntary leave
- **WHEN** a member is removed by an owner or leaves voluntarily
- **THEN** the system SHALL publish a `MemberLeftWatchSpace` event containing `WatchSpaceId`, `UserId`, and `LeftAtUtc`
