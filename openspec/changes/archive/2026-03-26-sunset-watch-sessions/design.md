## Context

BloomWatch tracks anime in shared "watch spaces." Each tracked anime (`WatchSpaceAnime` aggregate) currently holds three child collections: participant entries, watch sessions, and metadata. Watch sessions record discrete viewing events (date, episode range, notes) and feed into analytics (shared stats). However, participant progress (individual `episodesWatched` + `individualStatus`) and shared status already capture what matters — how far each person is and the group consensus. Watch sessions duplicate this signal with extra manual effort.

The watch session feature spans three modules (AnimeTracking domain + application, Analytics read-side, and the Angular UI) plus a dedicated database table, EF Core configuration, API endpoint, and tests.

## Goals / Non-Goals

**Goals:**
- Remove `WatchSession` entity, table, configuration, and all code that references it
- Remove the `POST .../sessions` endpoint
- Simplify `GET .../anime/{id}` detail response (drop `watchSessions` array)
- Simplify shared-stats response (drop `totalWatchSessions` and `mostRecentSessionDate`)
- Remove session-related UI (history list, log-session modal/form)
- Remove analytics data source method `GetWatchSessionAggregateAsync`
- Drop the `watch_sessions` table via EF migration

**Non-Goals:**
- Re-designing participant progress tracking (it stays as-is)
- Adding any replacement feature for session logging
- Changing any other analytics metrics (compatibility, rating gaps)
- Migrating existing watch session data to another structure — data is simply dropped

## Decisions

### 1. Hard removal vs. soft-deprecation

**Decision**: Hard removal — delete the entity, table, endpoint, and UI in one change.

**Rationale**: The feature is young with minimal user data. A soft-deprecation (mark deprecated, hide UI, drop later) adds complexity for no practical benefit. A clean removal keeps the codebase lean.

**Alternatives considered**: Feature-flag the UI while keeping the backend → rejected because maintaining dead code costs more than removing it.

### 2. Database table removal via EF migration

**Decision**: Generate a new EF Core migration that drops the `watch_sessions` table in the `anime_tracking` schema.

**Rationale**: Consistent with how the project manages schema changes. The migration is straightforward (`migrationBuilder.DropTable`).

### 3. Aggregate cleanup approach

**Decision**: Remove the `_watchSessions` collection, `RecordWatchSession` method, and the `WatchSessions` navigation from `WatchSpaceAnime`. Remove `WatchSession` entity entirely.

**Rationale**: No remaining code references these after the endpoint and analytics query are removed.

### 4. Shared-stats response simplification

**Decision**: Remove `totalWatchSessions` and `mostRecentSessionDate` from `SharedStatsResult` and the handler. The response shrinks to three fields: `totalEpisodesWatchedTogether`, `totalFinished`, `totalDropped`.

**Rationale**: These fields are solely derived from watch sessions. Without the underlying data, they have no source.

**Alternative considered**: Replace `totalWatchSessions` with a derived metric from participant progress → rejected; this would be a new feature, not a removal.

### 5. Analytics page / UI cleanup

**Decision**: Remove the "Watch Sessions" stat card and "Most Recent Session" row from the analytics page. Remove the session history section and "Log Session" modal from the anime detail view.

**Rationale**: No data to display after removal.

## Risks / Trade-offs

- **Data loss**: Existing watch session records are permanently deleted → Acceptable given the feature is early-stage. If needed later, the table can be restored from a database backup before the migration runs.
- **Breaking API change**: Clients consuming `watchSessions` in the detail response or `totalWatchSessions`/`mostRecentSessionDate` in shared-stats will break → Only consumer is the BloomWatch Angular SPA, which is updated in the same change.
- **Analytics dashboard shrinks**: Two stat cards disappear from the analytics page → The remaining cards (episodes together, finished, dropped, compatibility, rating gaps) still provide meaningful insight.

## Migration Plan

1. Remove UI components (session list, modal, form, model interfaces, service method)
2. Remove API endpoint mapping and handler
3. Remove domain entity, aggregate method, and EF configuration
4. Generate EF migration to drop `watch_sessions` table
5. Remove analytics data source method and simplify shared-stats handler/result
6. Update/remove affected tests
7. Apply migration to the database

**Rollback**: Revert the commit and run `dotnet ef migrations remove` to undo the migration before it hits production. If already applied, restore the table from backup.
