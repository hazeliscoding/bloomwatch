## 1. Module Scaffolding

- [x] 1.1 Create the four AnimeTracking module projects (Domain, Application, Infrastructure, Contracts) with correct namespaces and add them to `BloomWatch.sln`
- [x] 1.2 Add project references: Domain ← Application ← Infrastructure, Contracts standalone; Infrastructure references WatchSpaces.Infrastructure and AniListSync.Infrastructure for cross-module reads
- [x] 1.3 Register AnimeTracking services in `BloomWatch.Api` (DI, DbContext, endpoint mapping)

## 2. Domain Layer

- [x] 2.1 Create `WatchSpaceAnimeId` strongly-typed ID value object (`readonly record struct` wrapping `Guid`)
- [x] 2.2 Create `AnimeStatus` enum (Backlog, Watching, Finished, Paused, Dropped)
- [x] 2.3 Create `ParticipantEntry` entity with fields: `ParticipantEntryId`, `WatchSpaceAnimeId`, `UserId`, `IndividualStatus`, `EpisodesWatched`, `RatingScore`, `RatingNotes`, `LastUpdatedAtUtc`
- [x] 2.4 Create `WatchSpaceAnime` aggregate root with all fields (Id, WatchSpaceId, AniListMediaId, metadata snapshots, mood/vibe/pitch, shared status/progress, timestamps) and `Create()` factory method that enforces the invariant of creating the initial ParticipantEntry
- [x] 2.5 Create `AnimeTrackingDomainException` base exception and `AnimeAlreadyInWatchSpaceException`
- [x] 2.6 Create `IAnimeTrackingRepository` interface with `AddAsync`, `ExistsAsync(watchSpaceId, anilistMediaId)`, and `SaveChangesAsync`

## 3. Application Layer

- [x] 3.1 Create `IMembershipChecker` abstraction interface with `IsMemberAsync(Guid watchSpaceId, Guid userId)`
- [x] 3.2 Create `IMediaCacheLookup` abstraction interface with `GetByAnilistMediaIdAsync(int anilistMediaId)` returning a DTO with the snapshot fields
- [x] 3.3 Create `AddAnimeToWatchSpaceCommand` record (`int AniListMediaId`, `string? Mood`, `string? Vibe`, `string? Pitch`, `Guid WatchSpaceId`, `Guid RequestingUserId`)
- [x] 3.4 Create `AddAnimeToWatchSpaceResult` record (`Guid WatchSpaceAnimeId`, metadata snapshot fields)
- [x] 3.5 Create `AddAnimeToWatchSpaceCommandHandler` implementing the orchestration: membership check → duplicate check → media lookup → aggregate creation → persist → return result

## 4. Infrastructure Layer

- [x] 4.1 Create `AnimeTrackingDbContext` with `DbSet<WatchSpaceAnime>` configured for the `anime_tracking` schema
- [x] 4.2 Create `WatchSpaceAnimeConfiguration` EF Core entity configuration (table mapping, column types, unique constraint on `(watch_space_id, anilist_media_id)`, owned entity for ParticipantEntry collection)
- [x] 4.3 Create `ParticipantEntryConfiguration` EF Core entity configuration
- [x] 4.4 Create `EfAnimeTrackingRepository` implementing `IAnimeTrackingRepository`
- [x] 4.5 Implement `MembershipChecker` (queries WatchSpaces schema `watch_space_members` table directly)
- [x] 4.6 Implement `MediaCacheLookup` (queries AniListSync schema `media_cache` table directly)
- [x] 4.7 Create `ServiceCollectionExtensions` for DI registration of DbContext, repository, and cross-module abstractions
- [x] 4.8 Generate initial EF Core migration for the `anime_tracking` schema

## 5. API Endpoint

- [x] 5.1 Create `AddAnimeRequest` DTO and `AddAnimeResponse` DTO in the API project
- [x] 5.2 Create `AnimeTrackingEndpoints` with `POST /watchspaces/{watchSpaceId}/anime` mapping that extracts the authenticated user ID, calls the handler, and maps results to HTTP status codes (201, 401, 403, 404, 409)
- [x] 5.3 Register the endpoint group in `Program.cs`

## 6. Verification

- [x] 6.1 Ensure the solution builds with `dotnet build`
- [x] 6.2 Verify the EF Core migration applies cleanly with `dotnet ef database update`
- [x] 6.3 Write unit tests for `WatchSpaceAnime.Create()` factory method (verifies aggregate invariants, metadata snapshot, initial participant entry)
- [x] 6.4 Write unit tests for `AddAnimeToWatchSpaceCommandHandler` (happy path, non-member 403, duplicate 409, cache miss 404)
