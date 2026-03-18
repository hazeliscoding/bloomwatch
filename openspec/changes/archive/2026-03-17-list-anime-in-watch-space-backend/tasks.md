## 1. Application Layer — Query and Result DTOs

- [x] 1.1 Create `ListWatchSpaceAnimeQuery` record in `Application/UseCases/ListWatchSpaceAnime/` with `WatchSpaceId` (Guid), optional `Status` (AnimeStatus?), and `RequestingUserId` (Guid)
- [x] 1.2 Create `ListWatchSpaceAnimeResult` record with `IReadOnlyList<WatchSpaceAnimeListItem> Items`
- [x] 1.3 Create `WatchSpaceAnimeListItem` record with: `WatchSpaceAnimeId` (Guid), `AnilistMediaId` (int), `PreferredTitle` (string), `CoverImageUrlSnapshot` (string?), `EpisodeCountSnapshot` (int?), `SharedStatus` (string), `SharedEpisodesWatched` (int), `AddedAtUtc` (DateTime), `Participants` (IReadOnlyList\<ParticipantSummary\>)
- [x] 1.4 Create `ParticipantSummary` record with: `UserId` (Guid), `IndividualStatus` (string), `EpisodesWatched` (int)

## 2. Domain Layer — Repository Extension

- [x] 2.1 Add `ListByWatchSpaceAsync(Guid watchSpaceId, AnimeStatus? statusFilter, CancellationToken)` method to `IAnimeTrackingRepository` returning `Task<IReadOnlyList<WatchSpaceAnime>>`

## 3. Application Layer — Query Handler

- [x] 3.1 Create `ListWatchSpaceAnimeQueryHandler` in `Application/UseCases/ListWatchSpaceAnime/` with `IMembershipChecker` and `IAnimeTrackingRepository` injected via primary constructor
- [x] 3.2 Implement `HandleAsync`: verify membership (throw `NotAWatchSpaceMemberException` if not member), call `ListByWatchSpaceAsync`, map aggregates to `ListWatchSpaceAnimeResult`

## 4. Infrastructure Layer — Repository Implementation

- [x] 4.1 Implement `ListByWatchSpaceAsync` in `EfAnimeTrackingRepository`: query `WatchSpaceAnimes` filtered by `WatchSpaceId`, optionally filter by `SharedStatus`, `.Include(a => a.ParticipantEntries)`, order by `AddedAtUtc` descending, return as `IReadOnlyList`

## 5. API Layer — Endpoint Registration

- [x] 5.1 Add `ListAnimeAsync` handler method in `AnimeTrackingEndpoints` accepting `watchSpaceId` (Guid route param), optional `status` (AnimeStatus? query param), `ClaimsPrincipal`, `ListWatchSpaceAnimeQueryHandler`, and `CancellationToken`
- [x] 5.2 Register `GET /` on the existing route group in `MapAnimeTrackingEndpoints` with `WithName("ListWatchSpaceAnime")`, summary, and `.Produces<ListWatchSpaceAnimeResult>(200)` / `.Produces(403)` metadata
- [x] 5.3 Register `ListWatchSpaceAnimeQueryHandler` as scoped in `ServiceCollectionExtensions.AddAnimeTrackingModule()`

## 6. Unit Tests

- [x] 6.1 Create `ListWatchSpaceAnimeQueryHandlerTests` in `BloomWatch.Modules.AnimeTracking.UnitTests` with scenarios: returns items when anime exist, returns empty list when none exist, throws `NotAWatchSpaceMemberException` for non-members, applies status filter correctly
