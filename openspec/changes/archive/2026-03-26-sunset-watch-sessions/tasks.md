## 1. UI Cleanup — Angular

- [x] 1.1 Remove `WatchSessionDetail` interface and `watchSessions` field from `WatchSpaceAnimeDetail` in `src/BloomWatch.UI/src/app/features/watch-spaces/watch-space.model.ts`
- [x] 1.2 Remove `recordWatchSession` method from `src/BloomWatch.UI/src/app/features/watch-spaces/watch-space.service.ts`
- [x] 1.3 Remove watch session history section (session list), "Log Session" button, session modal, session form, and all session-related signals/methods from `src/BloomWatch.UI/src/app/features/watch-spaces/anime-detail.ts`
- [x] 1.4 Remove `totalWatchSessions`, `mostRecentSessionDate` display and "Watch Sessions" stat card from `src/BloomWatch.UI/src/app/features/watch-spaces/watch-space-analytics.html`; update the corresponding component model/interface
- [x] 1.5 Update `src/BloomWatch.UI/src/app/features/watch-spaces/anime-detail.spec.ts` — remove session-related test cases
- [x] 1.6 Update `src/BloomWatch.UI/src/app/features/watch-spaces/watch-space.service.spec.ts` — remove `recordWatchSession` test cases
- [x] 1.7 Update `src/BloomWatch.UI/src/app/features/watch-spaces/watch-space-analytics.spec.ts` — remove session stat test cases

## 2. API Endpoint Removal

- [x] 2.1 Remove the `POST /watchspaces/{id}/anime/{watchSpaceAnimeId}/sessions` route mapping from `src/BloomWatch.Api/Modules/AnimeTracking/AnimeTrackingEndpoints.cs`
- [x] 2.2 Remove the `RecordWatchSessionAsync` handler method from `src/BloomWatch.Api/Modules/AnimeTracking/AnimeTrackingEndpoints.cs`
- [x] 2.3 Remove `RecordWatchSessionCommandHandler` DI registration from `src/BloomWatch.Api/Program.cs` (if registered there)

## 3. Application Layer — AnimeTracking

- [x] 3.1 Delete the entire `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Application/UseCases/RecordWatchSession/` folder (command, handler, request, result)
- [x] 3.2 Remove `WatchSessionDto` and `watchSessions` mapping from `GetWatchSpaceAnimeDetailResult` in `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Application/UseCases/GetWatchSpaceAnimeDetail/GetWatchSpaceAnimeDetailResult.cs`
- [x] 3.3 Remove watch-session mapping from `GetWatchSpaceAnimeDetailQueryHandler` in `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Application/UseCases/GetWatchSpaceAnimeDetail/GetWatchSpaceAnimeDetailQueryHandler.cs`

## 4. Domain Layer — AnimeTracking

- [x] 4.1 Delete `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Domain/Entities/WatchSession.cs`
- [x] 4.2 Remove `_watchSessions` collection, `WatchSessions` property, and `RecordWatchSession` method from `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Domain/Aggregates/WatchSpaceAnime.cs`
- [x] 4.3 Remove `InvalidWatchSessionException` (or its watch-session-specific usages) from `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Domain/Exceptions/AnimeTrackingDomainException.cs`

## 5. Infrastructure Layer — AnimeTracking

- [x] 5.1 Delete `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Infrastructure/Persistence/Configurations/WatchSessionConfiguration.cs`
- [x] 5.2 Remove `WatchSessionConfiguration` registration from `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Infrastructure/Persistence/AnimeTrackingDbContext.cs`
- [x] 5.3 Remove watch-session navigation/include from `WatchSpaceAnimeConfiguration` in `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Infrastructure/Persistence/Configurations/WatchSpaceAnimeConfiguration.cs`
- [x] 5.4 Remove any watch-session includes from `src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Infrastructure/Persistence/Repositories/EfAnimeTrackingRepository.cs`
- [x] 5.5 Generate EF Core migration to drop the `watch_sessions` table: `dotnet ef migrations add DropWatchSessionsTable --project src/Modules/AnimeTracking/BloomWatch.Modules.AnimeTracking.Infrastructure --startup-project src/BloomWatch.Api`

## 6. Analytics Module Cleanup

- [x] 6.1 Remove `GetWatchSessionAggregateAsync` from `src/Modules/Analytics/BloomWatch.Modules.Analytics.Application/Abstractions/IWatchSpaceAnalyticsDataSource.cs`
- [x] 6.2 Remove `GetWatchSessionAggregateAsync` implementation from `src/Modules/Analytics/BloomWatch.Modules.Analytics.Infrastructure/CrossModule/WatchSpaceAnalyticsDataSource.cs`
- [x] 6.3 Remove `WatchSessions` DbSet and related config from `src/Modules/Analytics/BloomWatch.Modules.Analytics.Infrastructure/CrossModule/AnimeTrackingReadDbContext.cs`
- [x] 6.4 Remove `totalWatchSessions` and `mostRecentSessionDate` from `SharedStatsResult` in `src/Modules/Analytics/BloomWatch.Modules.Analytics.Application/UseCases/GetSharedStats/SharedStatsResult.cs`
- [x] 6.5 Remove watch-session aggregate call and result mapping from `GetSharedStatsQueryHandler` in `src/Modules/Analytics/BloomWatch.Modules.Analytics.Application/UseCases/GetSharedStats/GetSharedStatsQueryHandler.cs`

## 7. Test Cleanup

- [x] 7.1 Delete `tests/BloomWatch.Modules.AnimeTracking.IntegrationTests/RecordWatchSessionEndpointTests.cs`
- [x] 7.2 Remove watch-session test cases from `tests/BloomWatch.Modules.AnimeTracking.UnitTests/Domain/WatchSpaceAnimeTests.cs`
- [x] 7.3 Remove watch-session mapping tests from `tests/BloomWatch.Modules.AnimeTracking.UnitTests/Application/GetWatchSpaceAnimeDetailQueryHandlerTests.cs`
- [x] 7.4 Remove watch-session aggregate assertions from `tests/BloomWatch.Modules.Analytics.UnitTests/Application/SharedStatsQueryHandlerTests.cs`
- [x] 7.5 Remove watch-session seed data and assertions from `tests/BloomWatch.Modules.Analytics.IntegrationTests/SharedStatsEndpointTests.cs`
- [x] 7.6 Remove watch-session table seeding from `tests/BloomWatch.Modules.Analytics.IntegrationTests/AnalyticsWebAppFactory.cs`

## 8. Verify & Clean Up

- [x] 8.1 Build solution (`dotnet build`) and confirm zero errors
- [x] 8.2 Run all backend tests (`dotnet test`) and confirm pass
- [x] 8.3 Build Angular app (`ng build`) and confirm zero errors
- [x] 8.4 Run Angular tests (`ng test --watch=false`) and confirm pass
