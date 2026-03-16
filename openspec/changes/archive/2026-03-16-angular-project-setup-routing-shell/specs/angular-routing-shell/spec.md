## ADDED Requirements

### Requirement: Feature-based directory structure
The Angular project SHALL use a feature-based directory structure with three top-level folders inside `src/app/`: `core/` for singleton services and layout components, `shared/` for reusable UI components and models, and `features/` for lazy-loaded feature areas.

#### Scenario: Project directory structure exists
- **WHEN** a developer inspects `src/BloomWatch.UI/src/app/`
- **THEN** the directories `core/`, `shared/`, and `features/` SHALL exist
- **AND** `features/` SHALL contain subdirectories: `auth/`, `dashboard/`, `watch-spaces/`, `settings/`

### Requirement: Lazy-loaded routing for all MVP pages
The Angular app SHALL define lazy-loaded routes for all MVP pages. Each feature area SHALL have its own route configuration file loaded via `loadChildren` or `loadComponent` from the top-level `app.routes.ts`.

#### Scenario: All required routes are navigable
- **WHEN** the app is running
- **THEN** the following routes SHALL resolve without errors: `/`, `/login`, `/register`, `/watch-spaces`, `/watch-spaces/:id`, `/watch-spaces/:id/anime/:animeId`, `/settings`

#### Scenario: Feature routes are lazy-loaded
- **WHEN** a user navigates to a feature route for the first time
- **THEN** the corresponding feature chunk SHALL be loaded on demand (not included in the main bundle)

### Requirement: Shell layout for authenticated pages
A `ShellLayoutComponent` SHALL wrap all authenticated routes (`/`, `/watch-spaces/**`, `/settings`). It SHALL render a navigation bar and a content area containing a `<router-outlet>` for child routes.

#### Scenario: Authenticated route displays shell layout
- **WHEN** a user navigates to `/`
- **THEN** the page SHALL display a navigation bar and the dashboard content below it

#### Scenario: Nav bar contains navigation links
- **WHEN** the shell layout is rendered
- **THEN** the nav bar SHALL contain links to at least: Dashboard (`/`), Watch Spaces (`/watch-spaces`), and Settings (`/settings`)

### Requirement: Minimal layout for public pages
A `MinimalLayoutComponent` SHALL wrap public routes (`/login`, `/register`). It SHALL render only a `<router-outlet>` without the navigation bar.

#### Scenario: Public route displays minimal layout
- **WHEN** a user navigates to `/login`
- **THEN** the page SHALL NOT display a navigation bar

#### Scenario: Register route uses minimal layout
- **WHEN** a user navigates to `/register`
- **THEN** the page SHALL NOT display a navigation bar

### Requirement: App compiles and runs without errors
The Angular application SHALL compile successfully with `ng build` and serve without errors with `ng serve`.

#### Scenario: Clean build
- **WHEN** a developer runs `ng build` in the `src/BloomWatch.UI/` directory
- **THEN** the build SHALL complete with zero errors

#### Scenario: Dev server starts successfully
- **WHEN** a developer runs `ng serve` in the `src/BloomWatch.UI/` directory
- **THEN** the development server SHALL start and serve the app on `http://localhost:4200`
- **AND** navigating to `http://localhost:4200` SHALL render the app without console errors

### Requirement: Standalone components only
All components, directives, and pipes in the Angular project SHALL be standalone (no NgModules). The app SHALL use `provideRouter` and other `provide*` functions in `app.config.ts` rather than module-based configuration.

#### Scenario: No NgModule declarations
- **WHEN** a developer inspects the project source
- **THEN** there SHALL be no files containing `@NgModule` declarations
