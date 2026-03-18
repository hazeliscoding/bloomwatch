## ADDED Requirements

### Requirement: Dashboard summary endpoint
The system SHALL expose `GET /watchspaces/{id}/dashboard` as an authenticated, member-only endpoint that returns a composite JSON object containing watch space stats, currently-watching list, backlog highlights, rating-gap highlights, and compatibility score.

#### Scenario: Successful dashboard load for a watch space with data
- **WHEN** an authenticated member of watch space `{id}` sends `GET /watchspaces/{id}/dashboard` and the watch space contains anime in various statuses with participant ratings
- **THEN** the system SHALL return `200 OK` with a JSON object containing `stats`, `compatibility`, `currentlyWatching`, `backlogHighlights`, and `ratingGapHighlights` fields

#### Scenario: Dashboard for an empty watch space
- **WHEN** an authenticated member sends `GET /watchspaces/{id}/dashboard` and the watch space contains no anime
- **THEN** the system SHALL return `200 OK` with `stats` showing all zeroes, `compatibility` as `null`, and empty arrays for `currentlyWatching`, `backlogHighlights`, and `ratingGapHighlights`

### Requirement: Dashboard stats computation
The `stats` object SHALL contain: `totalShows` (total anime count across all statuses), `currentlyWatching` (count with `sharedStatus = Watching`), `finished` (count with `sharedStatus = Finished`), and `episodesWatchedTogether` (sum of all `sharedEpisodesWatched` across all anime in the watch space).

#### Scenario: Stats reflect all statuses
- **WHEN** a watch space has 3 anime with `Watching` status, 2 with `Finished`, 1 with `Backlog`, and 1 with `Dropped`
- **THEN** `stats.totalShows` SHALL be 7, `stats.currentlyWatching` SHALL be 3, `stats.finished` SHALL be 2

#### Scenario: Episodes watched together sums all anime
- **WHEN** a watch space has anime A with `sharedEpisodesWatched = 12`, anime B with `sharedEpisodesWatched = 5`, and anime C with `sharedEpisodesWatched = 0`
- **THEN** `stats.episodesWatchedTogether` SHALL be 17

#### Scenario: Stats with no anime
- **WHEN** a watch space has no anime
- **THEN** `stats.totalShows` SHALL be 0, `stats.currentlyWatching` SHALL be 0, `stats.finished` SHALL be 0, `stats.episodesWatchedTogether` SHALL be 0

### Requirement: Currently watching list
The `currentlyWatching` array SHALL contain up to 5 anime with `sharedStatus = Watching`, ordered by most recently added. Each entry SHALL include `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrl`, `sharedEpisodesWatched`, and `episodeCountSnapshot`.

#### Scenario: Fewer than 5 currently watching
- **WHEN** a watch space has 2 anime with `Watching` status
- **THEN** `currentlyWatching` SHALL contain exactly 2 entries

#### Scenario: More than 5 currently watching
- **WHEN** a watch space has 8 anime with `Watching` status
- **THEN** `currentlyWatching` SHALL contain exactly 5 entries, ordered by most recently added

#### Scenario: No currently watching anime
- **WHEN** a watch space has no anime with `Watching` status
- **THEN** `currentlyWatching` SHALL be an empty array

### Requirement: Backlog highlights
The `backlogHighlights` array SHALL contain up to 5 randomly selected anime with `sharedStatus = Backlog`. Each entry SHALL include `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrl`, and `format`.

#### Scenario: Fewer than 5 backlog items
- **WHEN** a watch space has 3 anime with `Backlog` status
- **THEN** `backlogHighlights` SHALL contain exactly 3 entries

#### Scenario: More than 5 backlog items
- **WHEN** a watch space has 10 anime with `Backlog` status
- **THEN** `backlogHighlights` SHALL contain exactly 5 randomly selected entries

#### Scenario: No backlog items
- **WHEN** a watch space has no anime with `Backlog` status
- **THEN** `backlogHighlights` SHALL be an empty array

### Requirement: Rating gap highlights
The `ratingGapHighlights` array SHALL contain up to 3 anime with the largest per-anime rating gap. The per-anime gap is the mean of absolute differences between all distinct pairs of members' ratings for that anime. Only anime where at least 2 members have submitted a `ratingScore` SHALL be considered. Each entry SHALL include `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrl`, `gap` (the computed gap value), and `ratings` (array of `{ userId, displayName, ratingScore }` for each rater).

#### Scenario: Multiple anime with different rating gaps
- **WHEN** anime A has ratings [8.0, 3.0] (gap = 5.0), anime B has ratings [7.0, 6.0] (gap = 1.0), anime C has ratings [9.0, 2.0] (gap = 7.0), and anime D has ratings [5.0, 4.0] (gap = 1.0)
- **THEN** `ratingGapHighlights` SHALL contain anime C (gap 7.0), anime A (gap 5.0), and one of B or D (gap 1.0), ordered by gap descending

#### Scenario: Anime with 3+ raters
- **WHEN** anime A has ratings from 3 users: [9.0, 5.0, 7.0]
- **THEN** the per-anime gap SHALL be the mean of |9.0-5.0|, |9.0-7.0|, |5.0-7.0| = mean of [4.0, 2.0, 2.0] = 2.67

#### Scenario: Fewer than 3 qualifying anime
- **WHEN** only 1 anime has 2+ ratings
- **THEN** `ratingGapHighlights` SHALL contain exactly 1 entry

#### Scenario: No anime with 2+ ratings
- **WHEN** no anime has 2 or more members with a `ratingScore`
- **THEN** `ratingGapHighlights` SHALL be an empty array

### Requirement: Compatibility score computation
The `compatibility` object SHALL be computed as: `score = max(0, round(100 - averageGap × 10))` where `averageGap` is the mean of all per-anime gaps (same formula as rating gap highlights). The `compatibility` object SHALL include `score` (int, 0–100), `averageGap` (decimal), `ratedTogetherCount` (number of anime with 2+ ratings), and `label` (string based on score range).

#### Scenario: High compatibility
- **WHEN** the average gap across all qualifying anime is 0.5
- **THEN** `compatibility.score` SHALL be 95, `compatibility.label` SHALL be a label for scores 90–100

#### Scenario: Low compatibility
- **WHEN** the average gap across all qualifying anime is 6.0
- **THEN** `compatibility.score` SHALL be 40, `compatibility.label` SHALL be a label for scores below 50

#### Scenario: Perfect compatibility
- **WHEN** all raters gave the same score on every anime (averageGap = 0)
- **THEN** `compatibility.score` SHALL be 100

#### Scenario: Very large gap clamped to zero
- **WHEN** the average gap is 12.0
- **THEN** `compatibility.score` SHALL be 0 (clamped, not negative)

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

### Requirement: Compatibility null when insufficient data
The system SHALL return `compatibility` as `null` when fewer than 2 members have rated any anime in the watch space. A `compatibilityMessage` field SHALL be set to "Not enough data" when compatibility is `null`.

#### Scenario: No rated anime
- **WHEN** no anime in the watch space has any `ratingScore` from any member
- **THEN** `compatibility` SHALL be `null` and `compatibilityMessage` SHALL be "Not enough data"

#### Scenario: Only one member has rated anime
- **WHEN** only one member has submitted ratings (no anime has 2+ raters)
- **THEN** `compatibility` SHALL be `null` and `compatibilityMessage` SHALL be "Not enough data"

#### Scenario: Two members have rated at least one anime
- **WHEN** at least one anime has ratings from 2 or more members
- **THEN** `compatibility` SHALL be a valid object with `score`, `averageGap`, `ratedTogetherCount`, and `label`

### Requirement: Membership enforcement for dashboard
The system SHALL verify that the requesting user is a member of the watch space before returning dashboard data. Membership SHALL be checked before any computation.

#### Scenario: Non-member requests dashboard
- **WHEN** an authenticated user who is NOT a member of watch space `{id}` sends `GET /watchspaces/{id}/dashboard`
- **THEN** the system SHALL return `403 Forbidden`

#### Scenario: Unauthenticated request
- **WHEN** an unauthenticated request sends `GET /watchspaces/{id}/dashboard`
- **THEN** the system SHALL return `401 Unauthorized`

### Requirement: Not-found handling for dashboard
The system SHALL return 404 when the specified watch space does not exist. Membership SHALL be checked before the existence check to prevent information leakage.

#### Scenario: Watch space does not exist
- **WHEN** an authenticated user sends `GET /watchspaces/{id}/dashboard` and no watch space with that ID exists
- **THEN** the system SHALL return `404 Not Found`

### Requirement: Dashboard response shape
The system SHALL return a JSON response with the following top-level fields: `stats` (object), `compatibility` (object or null), `compatibilityMessage` (string or null), `currentlyWatching` (array), `backlogHighlights` (array), and `ratingGapHighlights` (array).

#### Scenario: Full response shape with compatibility
- **WHEN** the dashboard endpoint returns successfully with sufficient rating data
- **THEN** the response SHALL contain `stats` with `totalShows`, `currentlyWatching`, `finished`, `episodesWatchedTogether`; `compatibility` with `score`, `averageGap`, `ratedTogetherCount`, `label`; `compatibilityMessage` as `null`; `currentlyWatching` array with entries containing `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrl`, `sharedEpisodesWatched`, `episodeCountSnapshot`; `backlogHighlights` array with entries containing `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrl`, `format`; `ratingGapHighlights` array with entries containing `watchSpaceAnimeId`, `preferredTitle`, `coverImageUrl`, `gap`, `ratings`

#### Scenario: Response shape without compatibility
- **WHEN** the dashboard endpoint returns successfully without sufficient rating data
- **THEN** `compatibility` SHALL be `null` and `compatibilityMessage` SHALL be "Not enough data", while all other fields SHALL be present
