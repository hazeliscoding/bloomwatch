## Why

Members often struggle to decide what to watch next from their backlog. A "Pick for me" widget removes the debate by randomly selecting an anime and displaying it with enough context (cover, title, episodes, mood/vibe/pitch) to make an instant decision. The backend endpoint already exists (`GET /watchspaces/{id}/analytics/random-pick`).

## What Changes

- Add TypeScript interfaces for `RandomPickResult` and `RandomPickAnimeResult` to the model file
- Add `getRandomPick(spaceId)` service method
- Create a standalone `bloom-backlog-picker` shared component that calls the random-pick endpoint and displays the result with cover image, title, episode count, mood/vibe/pitch badges, a "Reroll" button, and a "View Details" link
- Handle empty backlog state ("Your backlog is empty — add some anime first!") and loading state
- Embed the picker on the dashboard in a two-column layout alongside the compatibility section (matching the wireframe)

## Capabilities

### New Capabilities
- `backlog-picker`: Standalone backlog picker component with random pick, reroll, loading, empty state, and detail navigation

### Modified Capabilities
- `watch-space-dashboard`: Add the backlog picker to the dashboard in a two-column layout alongside the compatibility card

## Impact

- **New files:** `shared/ui/backlog-picker/bloom-backlog-picker.ts`, `.scss`, `.spec.ts`
- **Modified files:** `watch-space.model.ts` (new interfaces), `watch-space.service.ts` (new method), `watch-space-dashboard.html` (embed picker in two-col layout), `watch-space-dashboard.scss` (minor layout adjustments if needed)
- **APIs consumed:** `GET /watchspaces/{id}/analytics/random-pick`
