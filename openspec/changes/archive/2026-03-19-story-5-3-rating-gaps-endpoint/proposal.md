## Why

The dashboard returns only the top 3 rating-gap highlights. The frontend analytics page needs a full, paginated-ready list of all anime where members disagree, so users can explore every point of divergence. A dedicated lightweight endpoint avoids pulling the full dashboard payload and allows independent use in analytics views.

## What Changes

- Add `GET /watchspaces/{id}/analytics/rating-gaps` endpoint (member-only, authenticated)
- Create a new `GetRatingGapsQuery` / `GetRatingGapsQueryHandler` use case in the Analytics Application layer
- Reuse `CompatibilityComputer.ComputeAnimeGaps` for the gap computation, but return **all** qualifying anime (not capped at 3) with a secondary sort by title alphabetically for tie-breaking
- Resolve display names for raters via `IUserDisplayNameLookup`
- Return a `"Not enough data"` message flag when no anime has 2+ raters
- Add unit and integration tests

## Capabilities

### New Capabilities
- `rating-gaps`: Dedicated endpoint returning all anime with 2+ raters sorted by rating gap descending, with per-anime rater details

### Modified Capabilities

## Impact

- **Analytics Application layer**: New use case (`GetRatingGapsQuery`, `GetRatingGapsQueryHandler`), new result DTOs
- **Analytics Infrastructure layer**: New handler registered in DI
- **API layer**: New route added to `AnalyticsEndpoints`
- **Tests**: New integration tests for the endpoint; existing dashboard and compatibility tests remain unchanged
