## ADDED Requirements

### Requirement: Random backlog pick endpoint exists

The system SHALL expose a `GET /watchspaces/{watchSpaceId}/analytics/random-pick` endpoint that selects one anime at random from the watch space's backlog.

#### Scenario: Successful random pick from populated backlog

- **WHEN** an authenticated member of the watch space calls `GET /watchspaces/{watchSpaceId}/analytics/random-pick` and the backlog contains at least one anime
- **THEN** the system returns 200 OK with a JSON body containing a `pick` object with the fields: `watchSpaceAnimeId` (Guid), `preferredTitle` (string), `coverImageUrlSnapshot` (string, nullable), `episodeCountSnapshot` (int, nullable), `mood` (string, nullable), `vibe` (string, nullable), `pitch` (string, nullable); and `message` SHALL be `null`

#### Scenario: Each call may return a different result

- **WHEN** a member calls the endpoint multiple times against a backlog with more than one anime
- **THEN** the selection is non-deterministic and MAY return a different anime on each call (no sticky selection)

### Requirement: Empty backlog returns null pick with message

The system SHALL return a 200 OK response with a `null` pick and a descriptive message when the backlog is empty, rather than returning 404.

#### Scenario: Backlog is empty

- **WHEN** a member calls `GET /watchspaces/{watchSpaceId}/analytics/random-pick` and no anime in the watch space has `sharedStatus = Backlog`
- **THEN** the system returns 200 OK with `pick` set to `null` and `message` set to `"Backlog is empty"`

#### Scenario: Backlog is empty because all anime have non-backlog statuses

- **WHEN** the watch space contains anime but none have `sharedStatus = Backlog` (e.g., all are Watching or Finished)
- **THEN** the system returns 200 OK with `pick` set to `null` and `message` set to `"Backlog is empty"`

### Requirement: Membership authorization is enforced

The system SHALL restrict access to the random-pick endpoint to members of the watch space.

#### Scenario: Non-member receives 403

- **WHEN** an authenticated user who is NOT a member of the watch space calls `GET /watchspaces/{watchSpaceId}/analytics/random-pick`
- **THEN** the system returns 403 Forbidden

#### Scenario: Unauthenticated user receives 401

- **WHEN** an unauthenticated request is made to `GET /watchspaces/{watchSpaceId}/analytics/random-pick`
- **THEN** the system returns 401 Unauthorized

### Requirement: Randomness is server-side

The system SHALL perform random selection on the server. The selection logic MUST NOT be predictable or controllable by the client.

#### Scenario: No client-supplied seed or offset parameter influences selection

- **WHEN** a client calls the endpoint with arbitrary query parameters
- **THEN** the system ignores unknown parameters and selects randomly using server-side logic

### Requirement: Pick is scoped to backlog status only

The system SHALL only consider anime with `sharedStatus = Backlog` as candidates for the random pick. Anime with any other status (Watching, Finished, Dropped, OnHold, PlanToWatch, Paused) MUST be excluded.

#### Scenario: Only backlog items are candidates

- **WHEN** the watch space contains anime with mixed statuses including some with `sharedStatus = Backlog`
- **THEN** the system selects only from anime where `sharedStatus = Backlog`
