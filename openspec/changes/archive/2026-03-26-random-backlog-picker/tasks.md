## 1. Models and Service

- [x] 1.1 Add `RandomPickResult` and `RandomPickAnimeResult` interfaces to `watch-space.model.ts`
- [x] 1.2 Add `getRandomPick(spaceId)` method to `watch-space.service.ts`
- [x] 1.3 Write unit test in `watch-space.service.spec.ts` for `getRandomPick`

## 2. Backlog Picker Component

- [x] 2.1 Create `shared/ui/backlog-picker/bloom-backlog-picker.ts` with `spaceId` input, `picked` output event, fetch-on-init, reroll method, loading/pick/empty signals, and inline template
- [x] 2.2 Create `shared/ui/backlog-picker/bloom-backlog-picker.scss` with BEM styles for pick card (cover, info, badges, actions), empty state, loading skeleton, and reduced-motion support
- [x] 2.3 Write unit tests in `shared/ui/backlog-picker/bloom-backlog-picker.spec.ts`: renders pick with data, empty backlog message, reroll triggers new fetch, loading state, picked event emitted on View Details click, null optional fields handled

## 3. Dashboard Integration

- [x] 3.1 Import `BloomBacklogPickerComponent` in `watch-space-dashboard.ts`, wrap compatibility + picker sections in a `dashboard__two-col` layout in the template, wire up `(picked)` to `navigateToAnime()`
- [x] 3.2 Update `watch-space-dashboard.spec.ts` to verify `bloom-backlog-picker` element is present in the dashboard
