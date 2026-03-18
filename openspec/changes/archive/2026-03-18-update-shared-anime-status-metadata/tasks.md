## 1. Domain — Aggregate Mutation and Exception

- [x] 1.1 Add `InvalidSharedStateException` to `BloomWatch.Modules.AnimeTracking.Domain/Exceptions/` extending `AnimeTrackingDomainException`
- [x] 1.2 Add `UpdateSharedState(AnimeStatus? sharedStatus, int? sharedEpisodesWatched, string? mood, string? vibe, string? pitch)` method to `WatchSpaceAnime` aggregate — apply only non-null parameters, validate `sharedEpisodesWatched` >= 0 and <= `EpisodeCountSnapshot` (when known), throw `InvalidSharedStateException` on violations

## 2. Application — Command, Handler, and Request DTO

- [x] 2.1 Create `UpdateSharedAnimeStatusCommand` record with `WatchSpaceId`, `WatchSpaceAnimeId`, `RequestingUserId`, and nullable fields: `SharedStatus`, `SharedEpisodesWatched`, `Mood`, `Vibe`, `Pitch`
- [x] 2.2 Create `UpdateSharedAnimeStatusRequest` DTO for the API request body with nullable fields: `SharedStatus` (string), `SharedEpisodesWatched` (int?), `Mood`, `Vibe`, `Pitch`
- [x] 2.3 Create `UpdateSharedAnimeStatusCommandHandler` — check membership via `IMembershipChecker`, fetch aggregate via `GetByIdAsync`, call `UpdateSharedState()`, call `SaveChangesAsync()`, map to `GetWatchSpaceAnimeDetailResult`

## 3. API Endpoint and DI Registration

- [x] 3.1 Add `PATCH /{watchSpaceAnimeId:guid}` to `AnimeTrackingEndpoints` route group — parse request body, map to command, handle `NotAWatchSpaceMemberException` (403), null result (404), `AnimeTrackingDomainException` (400), and return 200 with detail result
- [x] 3.2 Register `UpdateSharedAnimeStatusCommandHandler` in `ServiceCollectionExtensions`

## 4. Tests

- [x] 4.1 Unit test `WatchSpaceAnime.UpdateSharedState` — partial update applies only provided fields, unchanged fields remain
- [x] 4.2 Unit test `UpdateSharedState` — negative episode count throws `InvalidSharedStateException`
- [x] 4.3 Unit test `UpdateSharedState` — episode count exceeding snapshot throws `InvalidSharedStateException`
- [x] 4.4 Unit test `UpdateSharedState` — episode count accepted when snapshot is null (no upper bound)
- [x] 4.5 Unit test `UpdateSharedAnimeStatusCommandHandler` — happy path returns updated detail
- [x] 4.6 Unit test handler — non-member throws `NotAWatchSpaceMemberException`
- [x] 4.7 Unit test handler — anime not found returns null
