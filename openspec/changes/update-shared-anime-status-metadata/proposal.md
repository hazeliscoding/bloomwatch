## Why

Watch space members can add and view anime, but they cannot yet update the group's shared tracking state — status, episode progress, mood, vibe, or pitch. Story 4.4 adds the backend PATCH endpoint so that any member can move an anime through its lifecycle (e.g. Backlog → Watching → Finished) and keep shared metadata current.

## What Changes

- Add `PATCH /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` endpoint accepting a partial update body with optional fields: `sharedStatus`, `sharedEpisodesWatched`, `mood`, `vibe`, `pitch`
- Add a domain mutation method on the `WatchSpaceAnime` aggregate that enforces validation rules:
  - `sharedStatus` must be a valid `AnimeStatus` enum value
  - `sharedEpisodesWatched` must be >= 0 and <= `EpisodeCountSnapshot` (when known)
  - Only provided (non-null) fields are applied (partial patch semantics)
- Add a new command/handler pair (`UpdateSharedAnimeStatus`) in the Application layer
- Enforce membership check (403 for non-members) and not-found handling (404)
- Return 200 OK with the updated record; return 400 for constraint violations

## Capabilities

### New Capabilities
- `update-shared-anime-status`: PATCH endpoint for updating shared anime status and metadata fields on a watch space anime, with partial-update semantics and domain validation

### Modified Capabilities
_(none — existing list and detail endpoints are read-only and unaffected)_

## Impact

- **API**: New `PATCH` endpoint on the existing `/watchspaces/{watchSpaceId}/anime` route group
- **Domain**: New public mutation method on `WatchSpaceAnime` aggregate; new domain exception for constraint violations
- **Application**: New command, handler, request DTO, and result DTO
- **Infrastructure**: No schema or migration changes — all target columns already exist
- **No breaking changes** to existing endpoints or contracts
