## Why

The AnimeTracking module has no implementation yet. Story 4.1 is the foundational entry point — without the ability to add anime to a watch space, none of the downstream tracking features (status updates, progress tracking, ratings, watch sessions) can function. This is the first write operation for the module and establishes the aggregate root, database schema, and cross-module integration patterns that all subsequent Epic 4 stories build upon.

## What Changes

- Create the `AnimeTracking` module with Domain, Application, Infrastructure, and Contracts projects
- Implement the `WatchSpaceAnime` aggregate root with a factory method for creation
- Implement the `ParticipantEntry` entity (created automatically for the adding user)
- Add `POST /watchspaces/{id}/anime` endpoint accepting `{ anilistMediaId, mood?, vibe?, pitch? }`
- Create `AnimeTrackingDbContext` with EF Core configurations and initial migration for the `anime_tracking` schema
- Enforce business rules: membership check (via WatchSpaces module), duplicate AniList ID prevention (409), metadata snapshot from AniListSync cache
- Set defaults: `sharedStatus = Backlog`, `sharedEpisodesWatched = 0`, `individualStatus = Backlog`, `episodesWatched = 0`
- Return 201 Created with the new `WatchSpaceAnimeId` and metadata snapshot

## Capabilities

### New Capabilities
- `add-anime-to-watch-space`: Adding an anime to a watch space by AniList media ID — includes aggregate creation, participant entry, cross-module membership verification, metadata snapshotting, and duplicate prevention.

### Modified Capabilities
_(none — this is a greenfield module)_

## Impact

- **New projects:** `BloomWatch.Modules.AnimeTracking.Domain`, `.Application`, `.Infrastructure`, `.Contracts` — following the same modular monolith pattern as WatchSpaces and AniListSync
- **API:** New `POST /watchspaces/{id}/anime` endpoint registered in `BloomWatch.Api`
- **Database:** New `anime_tracking` schema with `watch_space_anime` and `participant_entries` tables; unique constraint on `(watch_space_id, anilist_media_id)`
- **Cross-module reads:** WatchSpaces (membership verification), AniListSync (media cache lookup)
- **Dependencies:** References to `BloomWatch.Modules.WatchSpaces.Contracts` and `BloomWatch.Modules.AniListSync.Contracts`
- **Solution file:** Four new projects added to `BloomWatch.sln`
