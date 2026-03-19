### Requirement: Watch space list displays user's spaces
The system SHALL fetch and display all watch spaces the authenticated user belongs to when navigating to `/watch-spaces`. Each space card SHALL show the space name, the user's role, and the creation date.

#### Scenario: User has watch spaces
- **WHEN** an authenticated user navigates to `/watch-spaces` and they belong to one or more watch spaces
- **THEN** the system SHALL display a card for each watch space showing its name, the user's role (as a badge), and the creation date

#### Scenario: User has no watch spaces
- **WHEN** an authenticated user navigates to `/watch-spaces` and they belong to no watch spaces
- **THEN** the system SHALL display an empty state message encouraging the user to create their first watch space

#### Scenario: API error while loading spaces
- **WHEN** the `GET /watchspaces` request fails
- **THEN** the system SHALL display an error message indicating spaces could not be loaded

### Requirement: Loading state while fetching watch spaces
The system SHALL display a loading indicator while the watch space list is being fetched from the API.

#### Scenario: Spaces are loading
- **WHEN** the page is first rendered and the API request is in progress
- **THEN** the system SHALL display a loading indicator and SHALL NOT show the empty state or space cards

#### Scenario: Loading completes
- **WHEN** the API response is received
- **THEN** the system SHALL replace the loading indicator with the space list or empty state

### Requirement: Navigate to watch space detail
Each watch space card SHALL link to the watch space detail page at `/watch-spaces/:id`.

#### Scenario: User clicks a watch space card
- **WHEN** the user clicks on a watch space card
- **THEN** the system SHALL navigate to `/watch-spaces/{watchSpaceId}`

### Requirement: Create watch space via inline form
The system SHALL provide a "Create Watch Space" action that reveals an inline form with a name field and submit button.

#### Scenario: User opens the create form
- **WHEN** the user clicks the "Create Watch Space" button
- **THEN** the system SHALL display an inline form with a name input field and a submit button

#### Scenario: Successful creation
- **WHEN** the user enters a valid name and submits the form
- **THEN** the system SHALL send a `POST /watchspaces` request, add the newly created space to the displayed list, close the form, and clear the input

#### Scenario: Blank name validation
- **WHEN** the user attempts to submit the form with an empty or whitespace-only name
- **THEN** the system SHALL display a validation error and SHALL NOT send the API request

#### Scenario: API error during creation
- **WHEN** the `POST /watchspaces` request fails
- **THEN** the system SHALL display an error message near the form and keep the form open with the entered name preserved

#### Scenario: Cancel creation
- **WHEN** the user clicks a cancel action on the create form
- **THEN** the system SHALL hide the form and clear the input without making an API request

### Requirement: Watch space service encapsulates API calls
The system SHALL provide a `WatchSpaceService` that encapsulates all watch-space-related API calls using the existing `ApiService`.

#### Scenario: Fetch watch spaces
- **WHEN** `getMyWatchSpaces()` is called
- **THEN** the service SHALL send `GET /watchspaces` and return an `Observable<WatchSpaceSummary[]>`

#### Scenario: Create a watch space
- **WHEN** `createWatchSpace(name)` is called
- **THEN** the service SHALL send `POST /watchspaces` with `{ name }` and return an `Observable<WatchSpaceSummary>`

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
