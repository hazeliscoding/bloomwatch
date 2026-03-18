## Context

The `/watch-spaces` route exists and renders a placeholder `WatchSpaceList` component (`<h1>Watch Spaces</h1>`). The backend provides `GET /watchspaces` (returns `WatchSpaceSummary[]` with `WatchSpaceId`, `Name`, `CreatedAt`, `Role`) and `POST /watchspaces` (accepts `{ Name }`, returns the created space). The shared UI library has bloom-card, bloom-button, bloom-input, and bloom-badge components ready for use. The project uses Angular 21 with standalone components, signal-based state, and the kawaii/Y2K design system.

## Goals / Non-Goals

**Goals:**
- Display the user's watch spaces as a card grid at `/watch-spaces`
- Allow creating a new watch space via an inline create form
- Handle loading, empty, and error states
- Navigate to a specific watch space on card click

**Non-Goals:**
- Member count display (backend summary DTO doesn't include it yet)
- Watch space settings, renaming, or deletion (Story 8.2)
- Invitation management (Story 8.3)
- Watch space detail page content (separate story)

## Decisions

### 1. Rewrite `WatchSpaceList` in place rather than creating a new component

The existing `WatchSpaceList` is a placeholder wired into routes. Rewrite its contents directly.

**Rationale:** Avoids changing routes or creating dead code. The component and its route binding already exist.

### 2. Create `WatchSpaceService` in the feature directory

Place the service at `features/watch-spaces/watch-space.service.ts` rather than in `core/`.

**Rationale:** Watch-space API calls are feature-specific, not cross-cutting infrastructure like auth. Keeps the feature self-contained. Uses `ApiService` for HTTP calls internally.

**Alternative considered:** Putting it in `core/http/` — rejected because it's domain logic, not infrastructure.

### 3. TypeScript interfaces for DTOs in a local models file

Create `features/watch-spaces/watch-space.model.ts` with `WatchSpaceSummary` and `CreateWatchSpaceRequest` interfaces.

**Rationale:** Co-locates types with the feature that uses them. Avoids a central models folder that becomes a dumping ground.

### 4. Inline create form with toggle rather than a modal/dialog

Show/hide a "Create Watch Space" form inline at the top of the page (toggled by a button) instead of opening a modal dialog.

**Rationale:** Simpler to implement — no dialog infrastructure needed. The form has a single field (name), so a modal would be overkill. Can be extracted to a modal later if the UX demands it.

**Alternative considered:** A full modal component — deferred to when more complex create flows exist.

### 5. Signal-based component state, no NgRx or external state management

Use component-level signals for the watch space list, loading state, and create form state.

**Rationale:** The page is self-contained with simple state (fetch list, create item, add to list). No cross-component state sharing needed. Consistent with existing components (login, register).

### 6. Optimistic list update after creation

After a successful `POST /watchspaces`, append the returned space to the local list signal rather than re-fetching the full list.

**Rationale:** Faster UX — the user sees their new space immediately. The backend returns the full `CreateWatchSpaceResult` which maps to `WatchSpaceSummary`, so no data is lost.

### 7. Use bloom-badge to display user role

Show the user's role (Owner, Member) as a `bloom-badge` on each card.

**Rationale:** Leverages existing component. Role-based color coding (e.g., pink for Owner, blue for Member) provides quick visual differentiation.

## Risks / Trade-offs

- **[No member count]** The acceptance criteria mentions member count, but `GET /watchspaces` doesn't return it. → Acceptable: display available fields now. Backend can add `MemberCount` to the summary DTO later with no frontend breaking changes.

- **[Inline form simplicity]** An inline form may feel basic compared to a modal. → Mitigated: single-field forms work well inline. Can be upgraded to a dialog in future iterations.

- **[No pagination]** The list fetches all spaces at once. → Acceptable: users are unlikely to have hundreds of watch spaces. Pagination can be added if the need arises.
