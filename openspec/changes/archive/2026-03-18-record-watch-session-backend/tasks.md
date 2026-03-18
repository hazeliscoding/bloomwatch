## 1. Domain Layer

- [x] 1.1 Add `InvalidWatchSessionException` class extending `AnimeTrackingDomainException`
- [x] 1.2 Add internal creation constructor to `WatchSession` entity that accepts all fields and generates the `Id`
- [x] 1.3 Add `RecordWatchSession(DateTime sessionDateUtc, int startEpisode, int endEpisode, string? notes, Guid createdByUserId)` method on `WatchSpaceAnime` aggregate that validates episode range (startEpisode >= 1, endEpisode >= startEpisode), creates a `WatchSession`, and adds it to `_watchSessions`

## 2. Application Layer

- [x] 2.1 Create `RecordWatchSessionCommand` record with fields: `WatchSpaceId`, `WatchSpaceAnimeId`, `UserId`, `SessionDateUtc`, `StartEpisode`, `EndEpisode`, `Notes`
- [x] 2.2 Create `RecordWatchSessionRequest` DTO with fields: `SessionDateUtc`, `StartEpisode`, `EndEpisode`, `Notes`
- [x] 2.3 Create `RecordWatchSessionResult` record with fields: `Id`, `SessionDateUtc`, `StartEpisode`, `EndEpisode`, `Notes`, `CreatedByUserId`
- [x] 2.4 Create `RecordWatchSessionCommandHandler` that checks membership, loads the aggregate, calls `RecordWatchSession`, saves, and returns the result

## 3. API Endpoint

- [x] 3.1 Add `POST /{watchSpaceAnimeId}/sessions` route in `AnimeTrackingEndpoints` that maps to a `RecordWatchSessionAsync` handler method
- [x] 3.2 Wire up request deserialization, membership/not-found/domain exception handling, and return `201 Created` with session details

## 4. Testing

- [x] 4.1 Add domain unit tests for `WatchSpaceAnime.RecordWatchSession` covering: valid creation, startEpisode < 1, endEpisode < startEpisode, single-episode session, notes null
- [x] 4.2 Add integration tests for the POST endpoint covering: 201 success, 400 validation errors, 403 non-member, 404 anime not found, 401 unauthenticated

## 5. Docs

- [x] 5.1 Update user-stories.md to mark Story 4.7 as done and update sprint progress
