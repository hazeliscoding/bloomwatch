## ADDED Requirements

### Requirement: Compatibility score endpoint
The system SHALL expose `GET /watchspaces/{id}/analytics/compatibility` as an authenticated, member-only endpoint that returns the compatibility score for the specified watch space.

#### Scenario: Successful compatibility score with sufficient data
- **WHEN** an authenticated member of watch space `{id}` sends `GET /watchspaces/{id}/analytics/compatibility` and the watch space contains at least one anime rated by 2 or more members
- **THEN** the system SHALL return `200 OK` with a JSON object containing `compatibility` (non-null object with `score`, `averageGap`, `ratedTogetherCount`, `label`) and `message` as `null`

#### Scenario: Compatibility score with insufficient data
- **WHEN** an authenticated member sends `GET /watchspaces/{id}/analytics/compatibility` and no anime in the watch space has ratings from 2 or more members
- **THEN** the system SHALL return `200 OK` with `compatibility` as `null` and `message` as "Not enough data"

### Requirement: Compatibility score computation
The `compatibility.score` SHALL be computed as `max(0, round(100 - averageGap × 10))` where `averageGap` is the mean of all per-anime gaps. A per-anime gap is the mean of absolute differences between all distinct pairs of members' `ratingScore` values for that anime. Only anime where at least 2 members have submitted a non-null `ratingScore` SHALL be considered.

#### Scenario: Perfect compatibility
- **WHEN** all raters gave the same score on every qualifying anime (averageGap = 0)
- **THEN** `compatibility.score` SHALL be 100

#### Scenario: High gap yields low score
- **WHEN** the average gap across all qualifying anime is 6.0
- **THEN** `compatibility.score` SHALL be 40

#### Scenario: Very large gap clamped to zero
- **WHEN** the average gap is 12.0 (100 - 120 = -20)
- **THEN** `compatibility.score` SHALL be 0 (clamped, not negative)

#### Scenario: Fractional gap rounded
- **WHEN** the average gap is 0.5
- **THEN** `compatibility.score` SHALL be 95

### Requirement: Compatibility average gap and rated-together count
The response SHALL include `averageGap` (the mean of all per-anime gaps, rounded to 2 decimal places) and `ratedTogetherCount` (the number of anime with 2 or more raters).

#### Scenario: Average gap reflects all qualifying anime
- **WHEN** anime A has per-anime gap 2.0 and anime B has per-anime gap 4.0
- **THEN** `compatibility.averageGap` SHALL be 3.0 and `compatibility.ratedTogetherCount` SHALL be 2

#### Scenario: Anime with only one rater excluded
- **WHEN** anime A has ratings from 2 members and anime B has a rating from only 1 member
- **THEN** `compatibility.ratedTogetherCount` SHALL be 1 and anime B SHALL NOT contribute to `averageGap`

### Requirement: Compatibility labels
The `compatibility.label` SHALL be determined by the score range: 90–100 maps to "Very synced, with a little spice", 70–89 maps to "Pretty aligned", 50–69 maps to "Some differences", and below 50 maps to "Wildly different tastes".

#### Scenario: Score of 95
- **WHEN** `compatibility.score` is 95
- **THEN** `compatibility.label` SHALL be "Very synced, with a little spice"

#### Scenario: Score of 75
- **WHEN** `compatibility.score` is 75
- **THEN** `compatibility.label` SHALL be "Pretty aligned"

#### Scenario: Score of 55
- **WHEN** `compatibility.score` is 55
- **THEN** `compatibility.label` SHALL be "Some differences"

#### Scenario: Score of 30
- **WHEN** `compatibility.score` is 30
- **THEN** `compatibility.label` SHALL be "Wildly different tastes"

### Requirement: Membership enforcement
The system SHALL verify that the requesting user is a member of the watch space before returning compatibility data. Non-members SHALL receive `403 Forbidden`.

#### Scenario: Non-member requests compatibility
- **WHEN** an authenticated user who is NOT a member of watch space `{id}` sends `GET /watchspaces/{id}/analytics/compatibility`
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated request sends `GET /watchspaces/{id}/analytics/compatibility`
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Response shape
The endpoint SHALL return a JSON object with exactly two top-level fields: `compatibility` (object or null) and `message` (string or null). When `compatibility` is non-null it SHALL contain `score` (int, 0–100), `averageGap` (decimal), `ratedTogetherCount` (int), and `label` (string).

#### Scenario: Full response with compatibility data
- **WHEN** the endpoint returns successfully with sufficient rating data
- **THEN** the response SHALL have `compatibility` as an object with `score`, `averageGap`, `ratedTogetherCount`, `label`, and `message` as `null`

#### Scenario: Response without compatibility data
- **WHEN** the endpoint returns successfully without sufficient rating data
- **THEN** the response SHALL have `compatibility` as `null` and `message` as "Not enough data"
