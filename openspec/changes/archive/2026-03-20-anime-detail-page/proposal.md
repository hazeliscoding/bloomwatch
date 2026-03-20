## Why

The anime detail page is the core interaction surface for watch spaces — it's where users view full anime metadata, track shared and individual progress, submit ratings, and record watch sessions. The route (`/watch-spaces/:id/anime/:animeId`) already exists and navigates from the anime list, but the component is a placeholder stub. Without this page, users cannot interact with anime they've added beyond seeing them in a grid.

## What Changes

- Implement the `AnimeDetail` component as a full-featured detail page consuming the `GET /watchspaces/{watchSpaceId}/anime/{watchSpaceAnimeId}` endpoint
- Display anime metadata hero section (cover image, title, format, season/year, episode count)
- Show shared tracking state (status, episodes watched, mood, vibe, pitch) with inline editing for space members
- Render participant progress table showing each member's individual status, episodes watched, and rating
- Display watch session timeline/history
- Add action panels for: updating own progress (status + episodes), submitting/editing own rating (0.5–10.0 with notes), and recording a new watch session
- Add service methods to `WatchSpaceService` for the detail endpoint and mutation endpoints (update progress, update rating, record session)
- Add TypeScript interfaces for the detail response DTO

## Capabilities

### New Capabilities
- `anime-detail-view`: Frontend component rendering full anime detail with metadata hero, shared state, participants, sessions, and action panels

### Modified Capabilities
- `watch-space-anime-detail`: Existing backend spec — no requirement changes, but the frontend now consumes its API surface

## Impact

- **Frontend:** New component implementation in `anime-detail.ts` + new SCSS file, new interfaces in `watch-space.model.ts`, new service methods in `watch-space.service.ts`
- **Backend:** No changes — all required API endpoints already exist and are tested
- **Routing:** Already wired at `/:id/anime/:animeId` — no route changes needed
- **Design system:** Uses existing Bloom components (card, badge, button, input, modal) and design tokens
