### Requirement: Showcase route is registered only in development mode

The application SHALL register the `/showcase` route exclusively when running in a non-production environment (`environment.production === false`). In production builds the route SHALL NOT exist in the router configuration.

#### Scenario: Local development includes showcase route

- **WHEN** the Angular app is served locally with `ng serve` (development mode)
- **THEN** the `/showcase` route SHALL be present in the router and navigable

#### Scenario: Production build excludes showcase route

- **WHEN** the Angular app is built for production (`ng build`)
- **THEN** the `/showcase` route SHALL NOT be registered in the router
- **AND** navigating to `/showcase` SHALL result in a 404 / unmatched-route response

### Requirement: Showcase navigation link is visible only in development mode

The shell navigation bar SHALL display the "Showcase" link only when the application is running in development mode (`isDevMode() === true`). The link SHALL be completely removed from the DOM in production — not merely hidden.

#### Scenario: Navigation bar shows showcase link in development

- **WHEN** the user is authenticated and viewing any page in development mode
- **THEN** the shell navigation bar SHALL include a "Showcase" menu item linking to `/showcase`

#### Scenario: Navigation bar hides showcase link in production

- **WHEN** the user is authenticated and viewing any page in a production build
- **THEN** the shell navigation bar SHALL NOT render a "Showcase" menu item
