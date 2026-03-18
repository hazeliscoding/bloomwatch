## 1. Models & Service

- [x] 1.1 Create `features/watch-spaces/watch-space.model.ts` with `WatchSpaceSummary` and `CreateWatchSpaceRequest` interfaces matching the backend DTOs
- [x] 1.2 Create `features/watch-spaces/watch-space.service.ts` with `getMyWatchSpaces()` and `createWatchSpace(name)` methods using `ApiService`
- [x] 1.3 Create `features/watch-spaces/watch-space.service.spec.ts` with unit tests for both service methods

## 2. Watch Space List Component

- [x] 2.1 Rewrite `features/watch-spaces/watch-space-list.ts` with signal-based state (spaces list, loading, error), fetch on init, and card grid layout using bloom-card, bloom-badge, bloom-button
- [x] 2.2 Create `features/watch-spaces/watch-space-list.html` template with loading state, empty state, error state, and space cards grid
- [x] 2.3 Create `features/watch-spaces/watch-space-list.scss` with responsive card grid layout and state styling

## 3. Create Watch Space Form

- [x] 3.1 Add inline create form to the list component: toggle visibility, name input with bloom-input, submit/cancel buttons, blank-name validation, API submission, and optimistic list append on success

## 4. Testing

- [x] 4.1 Create `features/watch-spaces/watch-space-list.spec.ts` with tests for: loading state, spaces rendering, empty state, card navigation link, create form toggle, successful creation, blank name validation, and API error handling
- [x] 4.2 Run all tests to confirm no regressions
