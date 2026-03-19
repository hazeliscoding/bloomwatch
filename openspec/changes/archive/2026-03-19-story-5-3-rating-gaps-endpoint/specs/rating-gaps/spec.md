## ADDED Requirements

### Requirement: Rating gaps endpoint
The system SHALL expose `GET /watchspaces/{id}/analytics/rating-gaps` as an authenticated, member-only endpoint that returns all anime in the watch space where at least 2 members have submitted ratings, sorted by descending gap magnitude.

#### Scenario: Successful rating gaps with multiple anime
- **WHEN** an authenticated member of watch space `{id}` sends `GET /watchspaces/{id}/analytics/rating-gaps` and the watch space contains anime rated by 2 or more members
- **THEN** the system SHALL return `200 OK` with a JSON object containing `items` (non-empty array of rating gap entries) and `message` as `null`

#### Scenario: No qualifying anime
- **WHEN** an authenticated member sends `GET /watchspaces/{id}/analytics/rating-gaps` and no anime in the watch space has ratings from 2 or more members
- **THEN** the system SHALL return `200 OK` with `items` as an empty array and `message` as "Not enough data"

### Requirement: Rating gap computation
The per-anime gap SHALL be the mean of absolute differences between all distinct pairs of members' `ratingScore` values for that anime. Only anime where at least 2 members have submitted a non-null `ratingScore` SHALL be considered. Members with null `ratingScore` SHALL be excluded from the computation.

#### Scenario: Two raters
- **WHEN** anime A has ratings [8.0, 3.0] from 2 members
- **THEN** the per-anime gap SHALL be 5.0

#### Scenario: Three raters
- **WHEN** anime A has ratings [9.0, 5.0, 7.0] from 3 members
- **THEN** the per-anime gap SHALL be the mean of |9.0-5.0|, |9.0-7.0|, |5.0-7.0| = mean of [4.0, 2.0, 2.0] = 2.67

#### Scenario: Anime with only one rater excluded
- **WHEN** anime A has a rating from only 1 member
- **THEN** anime A SHALL NOT appear in the `items` array

### Requirement: Sorting order
The `items` array SHALL be sorted by gap descending. When two or more anime have equal gap values, they SHALL be sorted by `preferredTitle` ascending (alphabetical tie-break).

#### Scenario: Different gaps
- **WHEN** anime A has gap 5.0, anime B has gap 7.0, anime C has gap 1.0
- **THEN** `items` SHALL be ordered: anime B (7.0), anime A (5.0), anime C (1.0)

#### Scenario: Equal gaps tie-broken by title
- **WHEN** anime "Naruto" has gap 3.0 and anime "Bleach" has gap 3.0
- **THEN** `items` SHALL be ordered: "Bleach" (3.0), "Naruto" (3.0)

### Requirement: Rating gap item shape
Each entry in the `items` array SHALL include `watchSpaceAnimeId` (guid), `preferredTitle` (string), `coverImageUrl` (string or null), `gap` (decimal, rounded to 2 decimal places), and `ratings` (array of rater objects). Each rater object SHALL include `userId` (guid), `displayName` (string), and `ratingScore` (decimal).

#### Scenario: Full item shape
- **WHEN** anime A has ratings from 2 members with display names "Alice" and "Bob"
- **THEN** the item SHALL contain `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrl`, `gap`, and `ratings` with 2 entries each containing `userId`, `displayName`, and `ratingScore`

### Requirement: All qualifying anime returned
The endpoint SHALL return all anime with 2+ raters, not a limited subset. There SHALL be no cap on the number of items returned.

#### Scenario: Many qualifying anime
- **WHEN** a watch space has 20 anime each rated by 2+ members
- **THEN** `items` SHALL contain exactly 20 entries

### Requirement: Membership enforcement
The system SHALL verify that the requesting user is a member of the watch space before returning rating gap data. Non-members SHALL receive `403 Forbidden`.

#### Scenario: Non-member requests rating gaps
- **WHEN** an authenticated user who is NOT a member of watch space `{id}` sends `GET /watchspaces/{id}/analytics/rating-gaps`
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated request sends `GET /watchspaces/{id}/analytics/rating-gaps`
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Response shape
The endpoint SHALL return a JSON object with exactly two top-level fields: `items` (array) and `message` (string or null). When `items` is non-empty, `message` SHALL be `null`. When `items` is empty, `message` SHALL be "Not enough data".

#### Scenario: Response with data
- **WHEN** the endpoint returns successfully with qualifying anime
- **THEN** the response SHALL have `items` as a non-empty array and `message` as `null`

#### Scenario: Response without data
- **WHEN** the endpoint returns successfully without qualifying anime
- **THEN** the response SHALL have `items` as an empty array and `message` as "Not enough data"
