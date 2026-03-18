## Why

The frontend dashboard needs a single endpoint to display a complete watch space overview — stats, currently watching list, backlog highlights, rating gaps, and compatibility score — without making multiple round trips. This is the first endpoint in the new Analytics module (Epic 5).

## What Changes

- Create a new **Analytics module** (`BloomWatch.Modules.Analytics`) with Domain, Application, Infrastructure, and Contracts layers following the existing modular monolith conventions
- Add a `GET /watchspaces/{id}/dashboard` endpoint that returns a composite summary DTO
- Implement a `GetDashboardSummary` query + handler that cross-reads from the AnimeTracking module's data (anime statuses, participant entries, ratings, watch sessions)
- Compute stats: `totalShows`, `currentlyWatching` count, `finished` count, `episodesWatchedTogether` (sum of all `sharedEpisodesWatched`)
- Return up to 5 currently-watching anime with progress info
- Return up to 5 randomly selected backlog items
- Return up to 3 anime with the largest per-user rating gap
- Compute compatibility score: `max(0, round(100 - averageGap × 10))` with labels, returning `null` when fewer than 2 members have rated any anime
- Enforce membership — non-members receive 403

## Capabilities

### New Capabilities
- `dashboard-summary`: Aggregated watch space dashboard endpoint returning stats, currently-watching list, backlog highlights, rating-gap highlights, and compatibility score

### Modified Capabilities

## Impact

- **New module:** Analytics (Domain, Application, Infrastructure, Contracts)
- **New endpoint:** `GET /watchspaces/{id}/dashboard`
- **Cross-module reads:** Analytics reads from AnimeTracking data via an abstraction interface
- **Database:** New `analytics` schema (may be empty initially if no persisted analytics data is needed — all computation is read-time)
- **Registration:** New module must be registered in the API host's DI and endpoint mapping
