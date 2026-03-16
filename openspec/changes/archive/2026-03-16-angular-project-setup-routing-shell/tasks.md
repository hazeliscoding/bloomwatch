## 1. Directory Structure

- [x] 1.1 Create the feature-based directory structure under `src/BloomWatch.UI/src/app/`: `core/layout/`, `shared/ui/`, `shared/models/`, `features/auth/`, `features/dashboard/`, `features/watch-spaces/`, `features/settings/`
- [x] 1.2 Add `.gitkeep` in empty directories (`shared/ui/`, `shared/models/`) so they're tracked by git

## 2. Root Component Cleanup

- [x] 2.1 Replace the default Angular placeholder in `app.html` with just `<router-outlet />`
- [x] 2.2 Clean up `app.scss` (remove default styles) and remove `app.spec.ts`

## 3. Layout Components

- [x] 3.1 Create `ShellLayoutComponent` in `core/layout/shell-layout/` with a nav bar (links to Dashboard `/`, Watch Spaces `/watch-spaces`, Settings `/settings`) and a `<router-outlet>` content area
- [x] 3.2 Create `MinimalLayoutComponent` in `core/layout/minimal-layout/` with only a `<router-outlet>` and no nav bar

## 4. Feature Placeholder Components

- [x] 4.1 Create `DashboardComponent` placeholder in `features/dashboard/`
- [x] 4.2 Create `LoginComponent` and `RegisterComponent` placeholders in `features/auth/`
- [x] 4.3 Create `WatchSpaceListComponent`, `WatchSpaceDetailComponent`, and `AnimeDetailComponent` placeholders in `features/watch-spaces/`
- [x] 4.4 Create `SettingsComponent` placeholder in `features/settings/`

## 5. Routing Configuration

- [x] 5.1 Create `features/dashboard/dashboard.routes.ts` with the dashboard route
- [x] 5.2 Create `features/auth/auth.routes.ts` with `/login` and `/register` routes
- [x] 5.3 Create `features/watch-spaces/watch-spaces.routes.ts` with routes for list, detail (`/:id`), and anime detail (`/:id/anime/:animeId`)
- [x] 5.4 Create `features/settings/settings.routes.ts` with the settings route
- [x] 5.5 Configure `app.routes.ts` with lazy-loaded route groups: authenticated routes under `ShellLayoutComponent`, public routes under `MinimalLayoutComponent`

## 6. Verification

- [x] 6.1 Verify `ng build` completes with zero errors
- [x] 6.2 Verify all routes render the correct layout (shell vs. minimal) with placeholder content
- [x] 6.3 Verify no `@NgModule` declarations exist in the project
