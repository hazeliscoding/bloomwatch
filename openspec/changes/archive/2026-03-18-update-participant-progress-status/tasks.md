## 1. Domain — Aggregate Method, Entity Method, and Exception

- [x] 1.1 Add `InvalidParticipantProgressException` to `BloomWatch.Modules.AnimeTracking.Domain/Exceptions/` extending `AnimeTrackingDomainException`
- [x] 1.2 Add `internal Update(AnimeStatus individualStatus, int episodesWatched)` method to `ParticipantEntry` — sets `IndividualStatus`, `EpisodesWatched`, and `LastUpdatedAtUtc` to `DateTime.UtcNow`
- [x] 1.3 Add `internal static ParticipantEntry Create(WatchSpaceAnimeId watchSpaceAnimeId, Guid userId, AnimeStatus individualStatus, int episodesWatched)` factory method to `ParticipantEntry` for upsert creation
- [x] 1.4 Add `UpdateParticipantProgress(Guid userId, AnimeStatus individualStatus, int episodesWatched)` method to `WatchSpaceAnime` aggregate — find existing entry by `userId` or create via factory, validate `episodesWatched` >= 0 and <= `EpisodeCountSnapshot` (when known), throw `InvalidParticipantProgressException` on violations, return the updated `ParticipantEntry`

## 2. Application — Command, Handler, and Request DTO

- [x] 2.1 Create `UpdateParticipantProgressCommand` record with `WatchSpaceId`, `WatchSpaceAnimeId`, `RequestingUserId`, `IndividualStatus` (AnimeStatus), `EpisodesWatched` (int)
- [x] 2.2 Create `UpdateParticipantProgressRequest` DTO for the API request body with required fields: `IndividualStatus` (string), `EpisodesWatched` (int)
- [x] 2.3 Create `UpdateParticipantProgressResult` record mapping from `ParticipantEntry` to `ParticipantDetail` shape: `UserId`, `IndividualStatus` (string), `EpisodesWatched`, `RatingScore`, `RatingNotes`, `LastUpdatedAtUtc`
- [x] 2.4 Create `UpdateParticipantProgressCommandHandler` — check membership via `IMembershipChecker`, fetch aggregate via `GetByIdAsync`, call `UpdateParticipantProgress()`, call `SaveChangesAsync()`, map to result DTO

## 3. API Endpoint and DI Registration

- [x] 3.1 Add `PATCH /{watchSpaceAnimeId:guid}/participant-progress` to `AnimeTrackingEndpoints` route group — parse request body, extract user ID from claims, map to command, handle `NotAWatchSpaceMemberException` (403), null result (404), `AnimeTrackingDomainException` (400), return 200 with participant detail
- [x] 3.2 Register `UpdateParticipantProgressCommandHandler` in `ServiceCollectionExtensions`

## 4. Tests

- [x] 4.1 Unit test `WatchSpaceAnime.UpdateParticipantProgress` — happy path updates existing entry and returns it
- [x] 4.2 Unit test `UpdateParticipantProgress` — upsert creates new entry when none exists for user
- [x] 4.3 Unit test `UpdateParticipantProgress` — negative episode count throws `InvalidParticipantProgressException`
- [x] 4.4 Unit test `UpdateParticipantProgress` — episode count exceeding snapshot throws `InvalidParticipantProgressException`
- [x] 4.5 Unit test `UpdateParticipantProgress` — episode count accepted when snapshot is null (no upper bound)
- [x] 4.6 Unit test `UpdateParticipantProgressCommandHandler` — happy path returns updated participant detail
- [x] 4.7 Unit test handler — non-member throws `NotAWatchSpaceMemberException`
- [x] 4.8 Unit test handler — anime not found returns null
