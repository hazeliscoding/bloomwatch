## Why

After login, users land at `/watch-spaces` but currently see only a placeholder heading. There is no way to view existing watch spaces, navigate into one, or create a new one from the UI. The backend endpoints (`GET /watchspaces`, `POST /watchspaces`) are fully implemented and ready — this change builds the frontend page that consumes them.

## What Changes

- Replace the `WatchSpaceList` placeholder component with a fully functional selector page at `/watch-spaces`
- Create a `WatchSpaceService` to call watch-space API endpoints
- Display the user's watch spaces as cards showing name, role, and creation date
- Add a "Create Watch Space" flow (modal or inline form) with name validation
- Handle loading, empty, and error states gracefully
- Navigate into a watch space via card click/link (routes to `/watch-spaces/:id`)

**Note:** The backend `GET /watchspaces` summary DTO does not include member count. Cards will display available fields (name, role, created date). Member count can be added when the backend extends the summary DTO.

## Capabilities

### New Capabilities
- `watch-space-selector`: Frontend page for listing, creating, and navigating to watch spaces

### Modified Capabilities

_None — the backend watch-space-management spec is unchanged; this is a frontend-only addition._

## Impact

- **Components**: `WatchSpaceList` rewritten from placeholder to full implementation
- **Services**: New `WatchSpaceService` under `features/watch-spaces/`
- **Models**: New TypeScript interfaces for `WatchSpaceSummary` and `CreateWatchSpaceRequest`
- **Shared UI**: Consumes existing bloom-card, bloom-button, bloom-input, bloom-badge components
- **No backend changes**: Consumes existing `GET /watchspaces` and `POST /watchspaces` endpoints
- **No new dependencies**: Uses existing `ApiService` and shared UI library
