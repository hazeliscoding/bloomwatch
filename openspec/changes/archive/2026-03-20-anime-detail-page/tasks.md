## 1. TypeScript Interfaces & Service Methods

- [x] 1.1 Add `WatchSpaceAnimeDetail`, `ParticipantDetail`, and `WatchSessionDetail` interfaces to `watch-space.model.ts`
- [x] 1.2 Add `UpdateParticipantProgressRequest`, `UpdateParticipantRatingRequest`, and `RecordWatchSessionRequest` request interfaces to `watch-space.model.ts`
- [x] 1.3 Add `getAnimeDetail(spaceId, animeId)` method to `WatchSpaceService`
- [x] 1.4 Add `updateParticipantProgress(spaceId, animeId, body)` method to `WatchSpaceService`
- [x] 1.5 Add `updateParticipantRating(spaceId, animeId, body)` method to `WatchSpaceService`
- [x] 1.6 Add `recordWatchSession(spaceId, animeId, body)` method to `WatchSpaceService`

## 2. AnimeDetail Component — Structure & Data Loading

- [x] 2.1 Replace the placeholder `AnimeDetail` component with a standalone component that reads `watchSpaceId` and `animeId` from route params
- [x] 2.2 Add loading state with spinner, error state with retry button, and back navigation link to `/watch-spaces/:id`
- [x] 2.3 Fetch detail data via `WatchSpaceService.getAnimeDetail()` on init and expose it as a signal
- [x] 2.4 Fetch watch space members via `WatchSpaceService.getWatchSpaceById()` for participant display name resolution

## 3. AnimeDetail Component — Display Sections

- [x] 3.1 Build hero section: cover image (with placeholder fallback on error/null), preferred title, format, season/year, episode count
- [x] 3.2 Build shared tracking state section: status badge, episode progress indicator, mood/vibe/pitch (conditionally displayed)
- [x] 3.3 Build participants section: table/list of each participant with display name, individual status badge, episodes watched, rating score, and rating notes
- [x] 3.4 Build watch sessions section: chronological list (most recent first) with date, episode range, notes, creator name; empty state when no sessions

## 4. AnimeDetail Component — Mutation Forms

- [x] 4.1 Build inline progress update form: status select (Backlog/Watching/Finished/Paused/Dropped), episodes watched number input; validate episodes >= 0; submit calls `updateParticipantProgress`, re-fetches detail on success
- [x] 4.2 Build inline rating form: score input (0.5–10.0, step 0.5), notes textarea (max 1000 chars with counter); validate range and length; submit calls `updateParticipantRating`, re-fetches detail on success
- [x] 4.3 Build inline record session form: date input, start episode number, end episode number, optional notes textarea; validate startEpisode >= 1 and endEpisode >= startEpisode; submit calls `recordWatchSession`, re-fetches detail on success
- [x] 4.4 Add expand/collapse toggle for each action form section

## 5. Styling

- [x] 5.1 Create `anime-detail.scss` with BEM classes following the Bloom design system: hero layout, sections spacing, form styles, responsive breakpoints
- [x] 5.2 Apply design tokens (colors, spacing, radius, shadows, typography) and Bloom component styles (bloom-card, bloom-badge, bloom-button, bloom-input)
- [x] 5.3 Add reduced-motion support for any transitions/animations

## 6. Testing

- [x] 6.1 Write unit tests for the `AnimeDetail` component: loading/error/success states, display of all sections, form submissions, back navigation
- [x] 6.2 Write unit tests for the new `WatchSpaceService` methods
- [x] 6.3 Verify existing tests still pass (no regressions)
