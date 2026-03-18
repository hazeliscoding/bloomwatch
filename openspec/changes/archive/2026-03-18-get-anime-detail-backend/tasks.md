## 1. Domain — WatchSession Entity

- [x] 1.1 Create `WatchSession` entity in `BloomWatch.Modules.AnimeTracking.Domain/Entities/` with properties: `Id` (Guid), `WatchSpaceAnimeId`, `SessionDateUtc`, `StartEpisode`, `EndEpisode`, `Notes` (nullable), `CreatedByUserId`
- [x] 1.2 Add `_watchSessions` backing field and `WatchSessions` read-only collection to the `WatchSpaceAnime` aggregate

## 2. Persistence — WatchSession Table and Configuration

- [x] 2.1 Create `WatchSessionConfiguration` EF Core configuration mapping `WatchSession` to `watch_sessions` table with snake_case columns, FK to `watch_space_anime(id)` with cascade delete
- [x] 2.2 Register `WatchSessions` navigation on `WatchSpaceAnimeConfiguration` (same pattern as `ParticipantEntries`)
- [x] 2.3 Add `DbSet<WatchSession>` or register entity in `AnimeTrackingDbContext` if needed
- [x] 2.4 Generate EF Core migration for the new `watch_sessions` table

## 3. Repository — GetByIdAsync

- [x] 3.1 Add `GetByIdAsync(Guid watchSpaceId, WatchSpaceAnimeId id, CancellationToken)` to `IAnimeTrackingRepository` returning `WatchSpaceAnime?`
- [x] 3.2 Implement `GetByIdAsync` in `EfAnimeTrackingRepository` with `.Include(a => a.ParticipantEntries).Include(a => a.WatchSessions)` and filter by `watchSpaceId`

## 4. Application — Query and Handler

- [x] 4.1 Create `GetWatchSpaceAnimeDetailQuery` record with `WatchSpaceId`, `WatchSpaceAnimeId`, `RequestingUserId`
- [x] 4.2 Create `GetWatchSpaceAnimeDetailResult` with nested `ParticipantDetail` (userId, individualStatus, episodesWatched, ratingScore, ratingNotes, lastUpdatedAtUtc) and `WatchSessionDetail` (watchSessionId, sessionDateUtc, startEpisode, endEpisode, notes, createdByUserId)
- [x] 4.3 Create `GetWatchSpaceAnimeDetailQueryHandler` — check membership, call `GetByIdAsync`, return 404 if null, map to result DTO

## 5. API Endpoint

- [x] 5.1 Add `GET /{watchSpaceAnimeId:guid}` to `AnimeTrackingEndpoints` route group, wiring to the handler with 200/403/404 responses
- [x] 5.2 Register the handler in DI (`ServiceCollectionExtensions`)

## 6. Tests

- [x] 6.1 Unit test `GetWatchSpaceAnimeDetailQueryHandler` — happy path returns full detail with participants and sessions
- [x] 6.2 Unit test — non-member throws `NotAWatchSpaceMemberException`
- [x] 6.3 Unit test — anime not found returns null / handler maps to appropriate result
- [x] 6.4 Verify EF migration applies cleanly and `watch_sessions` table schema is correct
