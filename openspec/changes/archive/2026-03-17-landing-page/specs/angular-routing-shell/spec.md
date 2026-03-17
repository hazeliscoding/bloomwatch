## MODIFIED Requirements

### Requirement: Lazy-loaded routing for all MVP pages
The Angular app SHALL define lazy-loaded routes for all MVP pages. Each feature area SHALL have its own route configuration file loaded via `loadChildren` or `loadComponent` from the top-level `app.routes.ts`.

#### Scenario: All required routes are navigable
- **WHEN** the app is running
- **THEN** the following routes SHALL resolve without errors: `/`, `/login`, `/register`, `/watch-spaces`, `/watch-spaces/:id`, `/watch-spaces/:id/anime/:animeId`, `/settings`
- **AND** the `/` route SHALL render the landing page component inside `MinimalLayout`

#### Scenario: Feature routes are lazy-loaded
- **WHEN** a user navigates to a feature route for the first time
- **THEN** the corresponding feature chunk SHALL be loaded on demand (not included in the main bundle)

### Requirement: Minimal layout for public pages
A `MinimalLayoutComponent` SHALL wrap public routes (`/`, `/login`, `/register`). It SHALL render only a `<router-outlet>` without the navigation bar.

#### Scenario: Landing page uses minimal layout
- **WHEN** a user navigates to `/`
- **THEN** the page SHALL NOT display a navigation bar
- **AND** the landing page component SHALL be rendered

#### Scenario: Public route displays minimal layout
- **WHEN** a user navigates to `/login`
- **THEN** the page SHALL NOT display a navigation bar

#### Scenario: Register route uses minimal layout
- **WHEN** a user navigates to `/register`
- **THEN** the page SHALL NOT display a navigation bar

### Requirement: Feature-based directory structure
The Angular project SHALL use a feature-based directory structure with three top-level folders inside `src/app/`: `core/` for singleton services and layout components, `shared/` for reusable UI components and models, and `features/` for lazy-loaded feature areas.

#### Scenario: Project directory structure exists
- **WHEN** a developer inspects `src/BloomWatch.UI/src/app/`
- **THEN** the directories `core/`, `shared/`, and `features/` SHALL exist
- **AND** `features/` SHALL contain subdirectories: `auth/`, `dashboard/`, `watch-spaces/`, `settings/`, `landing/`
