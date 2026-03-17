### Requirement: Landing page route
The application SHALL serve a landing page at the `/` route. The landing page SHALL be a standalone Angular component rendered inside `MinimalLayout` (no navigation bar). It SHALL be lazy-loaded via `loadComponent`.

#### Scenario: Root URL renders landing page
- **WHEN** an unauthenticated user navigates to `/`
- **THEN** the landing page component SHALL render inside `MinimalLayout`
- **AND** the navigation bar SHALL NOT be displayed

#### Scenario: Landing page is lazy-loaded
- **WHEN** a user navigates to `/` for the first time
- **THEN** the landing page chunk SHALL be loaded on demand (not included in the main bundle)

### Requirement: Hero section with product introduction
The landing page SHALL display a hero section containing the product name "BloomWatch" as a prominent heading, a short tagline describing the product purpose, and a clear visual hierarchy that draws the user's attention.

#### Scenario: Hero heading is visible
- **WHEN** the landing page is rendered
- **THEN** an `<h1>` element SHALL display "BloomWatch" using the display font family and a gradient text treatment

#### Scenario: Tagline communicates product value
- **WHEN** the landing page is rendered
- **THEN** a subtitle paragraph SHALL appear below the heading communicating that BloomWatch is for tracking anime together

### Requirement: Call-to-action buttons
The landing page hero section SHALL contain two call-to-action buttons: a primary "Create Account" button linking to `/register` and a secondary/ghost "Sign In" button linking to `/login`.

#### Scenario: Primary CTA links to registration
- **WHEN** the landing page is rendered
- **THEN** a primary-variant `bloom-button` with label "Create Account" SHALL be visible
- **AND** clicking it SHALL navigate to `/register`

#### Scenario: Secondary CTA links to login
- **WHEN** the landing page is rendered
- **THEN** a ghost-variant `bloom-button` with label "Sign In" SHALL be visible
- **AND** clicking it SHALL navigate to `/login`

### Requirement: Feature highlights grid
The landing page SHALL display a grid of feature highlight cards below the hero section. Each card SHALL contain an icon, a title, and a short description. The grid SHALL present at least three feature highlights: shared backlog management, rating and comparison, and group discovery.

#### Scenario: Three feature cards are displayed
- **WHEN** the landing page is rendered
- **THEN** three `bloom-card` components SHALL be visible with titles "Shared Backlog", "Rate & Compare", and "Discover Together"

#### Scenario: Feature cards contain descriptive content
- **WHEN** a user inspects a feature card
- **THEN** each card SHALL contain a decorative icon, a heading, and a brief description paragraph

### Requirement: Responsive layout
The landing page layout SHALL be responsive. On viewports 640px and wider, the feature highlights grid SHALL display in a multi-column layout. On viewports narrower than 640px, the grid SHALL collapse to a single column, and font sizes SHALL scale down appropriately.

#### Scenario: Desktop viewport shows multi-column grid
- **WHEN** the viewport width is 640px or wider
- **THEN** the feature highlights grid SHALL render in multiple columns using `auto-fill` with a minimum column width of 200px

#### Scenario: Mobile viewport shows single-column grid
- **WHEN** the viewport width is narrower than 640px
- **THEN** the feature highlights grid SHALL render in a single column
- **AND** the hero title font size SHALL decrease to `--bloom-text-3xl`
- **AND** the landing container padding SHALL decrease

### Requirement: Design system compliance
The landing page SHALL use only semantic design tokens from the BloomWatch design system (`_tokens.scss`). It SHALL use `bloom-` prefixed shared components (`bloom-button`, `bloom-card`) and follow BEM naming conventions for its own SCSS classes. All text and interactive elements SHALL meet WCAG AA contrast requirements in both light and dark themes.

#### Scenario: Component uses design tokens
- **WHEN** a developer inspects the landing page SCSS
- **THEN** all spacing, color, font-size, and layout values SHALL reference `--bloom-*` custom properties
- **AND** no hard-coded color or spacing values SHALL be present

#### Scenario: Landing page renders correctly in dark mode
- **WHEN** the active theme is `'dark'`
- **THEN** the landing page text, cards, and background SHALL adapt using semantic token overrides with sufficient contrast
