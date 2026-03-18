## Why

Watch space members can update the group's shared anime status (Story 4.4), but each participant cannot yet track their own individual progress — episodes watched and personal status. Story 4.5 adds the backend PATCH endpoint so that each member can record how far they have personally gotten, enabling the group to see per-person progress at a glance.

## What Changes

- Add `PATCH /watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-progress` endpoint accepting `episodesWatched` (int) and `individualStatus` (enum string)
- Add an upsert-style domain method on the `WatchSpaceAnime` aggregate that finds or creates the caller's `ParticipantEntry` and updates progress
- Enforce validation: `individualStatus` must be a valid `AnimeStatus` value; `episodesWatched` must be >= 0 and <= `episodeCountSnapshot` (when known)
- Update `lastUpdatedAtUtc` on every successful mutation
- Add a new command/handler pair (`UpdateParticipantProgress`) in the Application layer
- Enforce membership check (403 for non-members) and not-found handling (404)
- Return 200 OK with the updated `ParticipantDetail`

## Capabilities

### New Capabilities
- `update-participant-progress`: PATCH endpoint for updating an individual participant's episode progress and personal status on a watch space anime, with upsert semantics and domain validation

### Modified Capabilities
_(none — existing endpoints are unaffected)_

## Impact

- **API**: New `PATCH` endpoint nested under the existing `/watchspaces/{id}/anime/{watchSpaceAnimeId}` route group
- **Domain**: New public method on `WatchSpaceAnime` aggregate for participant progress upsert; new internal update method on `ParticipantEntry` entity
- **Application**: New command, handler, and request DTO; reuses existing `ParticipantDetail` result DTO
- **Infrastructure**: No schema or migration changes — `participant_entries` table already exists with all required columns
- **No breaking changes** to existing endpoints or contracts
