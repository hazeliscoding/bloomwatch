## Why

Watch space members need to record their personal rating for an anime so their taste preferences are captured and can later feed into compatibility analytics (Story 9.x). The rating fields already exist on `ParticipantEntry` but there is no domain method or API endpoint to set them.

## What Changes

- Add a domain method on `WatchSpaceAnime` aggregate to update a participant's rating (score + optional notes) with upsert semantics
- Add a new domain exception for invalid rating values (out of range or wrong increment)
- Add a CQRS command + handler for the use case (`UpdateParticipantRating`)
- Expose `PATCH /watchspaces/{id}/anime/{watchSpaceAnimeId}/participant-rating` endpoint
- Validate `ratingScore` is between 0.5 and 10.0 inclusive in 0.5 increments; `ratingNotes` max 1000 characters

## Capabilities

### New Capabilities
- `submit-participant-rating`: Allows a watch space member to submit or overwrite their personal rating (score and optional notes) for an anime via a dedicated PATCH endpoint, with 0.5-increment scale validation and upsert semantics

### Modified Capabilities
_(none — existing specs are unaffected; the `update-participant-progress` capability covers status/episodes only)_

## Impact

- **Domain layer**: New `UpdateParticipantRating` method on `WatchSpaceAnime`, new `InvalidRatingException`
- **Application layer**: New `UpdateParticipantRatingCommand` / `UpdateParticipantRatingCommandHandler`, new request/result DTOs
- **API layer**: New PATCH endpoint registered in `AnimeTrackingEndpoints`
- **Database**: No migration needed — `rating_score` and `rating_notes` columns already exist on `participant_entries`
