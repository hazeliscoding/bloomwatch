## Why

The dashboard summary endpoint (Story 5.1) returns compatibility as one field among many. The frontend needs a lightweight, dedicated endpoint to fetch just the compatibility score — for example, in a watch space header badge or a standalone analytics card — without pulling the full dashboard payload. A focused endpoint also enables independent caching and polling in the future.

## What Changes

- Add `GET /watchspaces/{id}/analytics/compatibility` endpoint (member-only, authenticated)
- Extract the shared compatibility/gap computation logic from `GetDashboardSummaryQueryHandler` into a reusable static helper so both the dashboard and the new endpoint use the same code path
- Create a new `GetCompatibilityQuery` / `GetCompatibilityQueryHandler` use case in the Analytics Application layer
- Create a dedicated `CompatibilityScoreResult` response DTO with `score`, `averageGap`, `ratedTogetherCount`, `label`, and a nullable message when data is insufficient
- Add unit and integration tests for the new endpoint

## Capabilities

### New Capabilities
- `compatibility-score`: Dedicated endpoint returning the compatibility score, average gap, rated-together count, and label for a watch space

### Modified Capabilities
- `dashboard-summary`: Refactor to delegate compatibility computation to the new shared helper (no behavioural change — internal restructure only, no spec update needed)

## Impact

- **Analytics Application layer**: New use case (`GetCompatibilityQuery`), new shared helper class, existing `GetDashboardSummaryQueryHandler` refactored to call the helper
- **Analytics Infrastructure layer**: New handler registered in DI
- **API layer**: New route added to `AnalyticsEndpoints`
- **Tests**: New unit tests for the query handler, new integration tests for the endpoint, existing dashboard tests remain unchanged
