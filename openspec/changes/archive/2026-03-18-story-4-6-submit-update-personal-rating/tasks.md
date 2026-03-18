## 1. Domain — Entity Method, Exception, and Aggregate Method

- [x] 1.1 Add `InvalidRatingException` to `BloomWatch.Modules.AnimeTracking.Domain/Exceptions/` extending `AnimeTrackingDomainException`
- [x] 1.2 Add `internal void UpdateRating(decimal ratingScore, string? ratingNotes)` method to `ParticipantEntry` — sets `RatingScore`, `RatingNotes`, and `LastUpdatedAtUtc` to `DateTime.UtcNow`
- [x] 1.3 Add `UpdateParticipantRating(Guid userId, decimal ratingScore, string? ratingNotes)` method to `WatchSpaceAnime` aggregate — find existing entry by `userId` or create via existing `ParticipantEntry.Create` factory (with defaults `Backlog`, 0 episodes), validate `ratingScore` is 0.5–10.0 in 0.5 increments, validate `ratingNotes` <= 1000 chars when provided, throw `InvalidRatingException` on violations, call `UpdateRating()`, return the updated `ParticipantEntry`

## 2. Application — Command, Handler, and DTOs

- [x] 2.1 Rename `UpdateParticipantProgressResult` to `ParticipantDetailResult` (shared between Story 4.5 and 4.6) and update all references in the progress handler and endpoint
- [x] 2.2 Create `UpdateParticipantRatingRequest` DTO for the API request body with required field `RatingScore` (decimal) and optional field `RatingNotes` (string?)
- [x] 2.3 Create `UpdateParticipantRatingCommand` record with `WatchSpaceId`, `WatchSpaceAnimeId`, `RequestingUserId`, `RatingScore` (decimal), `RatingNotes` (string?)
- [x] 2.4 Create `UpdateParticipantRatingCommandHandler` — check membership via `IMembershipChecker`, fetch aggregate via `GetByIdAsync`, call `UpdateParticipantRating()`, call `SaveChangesAsync()`, map to `ParticipantDetailResult`

## 3. API Endpoint and DI Registration

- [x] 3.1 Add `PATCH /{watchSpaceAnimeId:guid}/participant-rating` to `AnimeTrackingEndpoints` route group — parse request body, extract user ID from claims, map to command, handle `NotAWatchSpaceMemberException` (403), null result (404), `AnimeTrackingDomainException` (400), return 200 with participant detail
- [x] 3.2 Register `UpdateParticipantRatingCommandHandler` in `ServiceCollectionExtensions`

## 4. Tests

- [x] 4.1 Unit test `WatchSpaceAnime.UpdateParticipantRating` — happy path updates existing entry's rating and returns it
- [x] 4.2 Unit test `UpdateParticipantRating` — upsert creates new entry with default progress when none exists for user
- [x] 4.3 Unit test `UpdateParticipantRating` — rating below 0.5 throws `InvalidRatingException`
- [x] 4.4 Unit test `UpdateParticipantRating` — rating above 10.0 throws `InvalidRatingException`
- [x] 4.5 Unit test `UpdateParticipantRating` — rating not in 0.5 increments throws `InvalidRatingException`
- [x] 4.6 Unit test `UpdateParticipantRating` — rating notes exceeding 1000 characters throws `InvalidRatingException`
- [x] 4.7 Unit test `UpdateParticipantRating` — null ratingNotes clears existing notes
- [x] 4.8 Unit test `UpdateParticipantRating` — omitted ratingNotes preserves existing notes
- [x] 4.9 Unit test `UpdateParticipantRatingCommandHandler` — happy path returns updated participant detail
- [x] 4.10 Unit test handler — non-member throws `NotAWatchSpaceMemberException`
- [x] 4.11 Unit test handler — anime not found returns null

## 5. Documentation

- [x] 5.1 Update `docs/user-stories.md` — mark Story 4.6 status as Done
