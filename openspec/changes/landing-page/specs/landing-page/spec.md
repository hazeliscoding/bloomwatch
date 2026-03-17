## ADDED Requirements

### Requirement: Landing page route
The application SHALL serve a landing page at the root path `/`. The page SHALL be publicly accessible without authentication.

#### Scenario: Root path renders landing page
- **WHEN** an unauthenticated user navigates to `/`
- **THEN** the landing page component SHALL be rendered inside the `MinimalLayout` (no navigation bar)

#### Scenario: Landing page is lazy-loaded
- **WHEN** a user navigates to `/` for the first time
- **THEN** the landing page chunk SHALL be loaded on demand (not included in the main bundle)

### Requirement: Hero section with product introduction
The landing page SHALL display a hero section containing the product name "BloomWatch" as a gradient-text heading, a subtitle describing the product, and two call-to-action buttons.

#### Scenario: Hero section content is visible
- **WHEN** the landing page is rendered
- **THEN** the page SHALL display a heading with the text "BloomWatch"
- **AND** the heading SHALL use the `gradient-text` and display font styles
- **AND** a subtitle SHALL be visible below the heading

#### Scenario: Hero section has accessible heading structure
- **WHEN** a screen reader parses the landing page
- **THEN** the product name SHALL be an `<h1>` element

### Requirement: Call-to-action buttons
The hero section SHALL contain two CTA buttons: a primary "Create Account" button linking to `/register` and a ghost "Sign In" button linking to `/login`. Both buttons SHALL use the `lg` size variant.

#### Scenario: Create Account button navigates to register
- **WHEN** a user clicks the "Create Account" button
- **THEN** the application SHALL navigate to `/register`

#### Scenario: Sign In button navigates to login
- **WHEN** a user clicks the "Sign In" button
- **THEN** the application SHALL navigate to `/login`

#### Scenario: CTA buttons use correct variants
- **WHEN** the landing page is rendered
- **THEN** the "Create Account" button SHALL use the `primary` variant with `lg` size
- **AND** the "Sign In" button SHALL use the `ghost` variant with `lg` size

### Requirement: Feature highlights grid
The landing page SHALL display a section of three feature highlight cards below the hero section. Each card SHALL contain an icon, a title, and a short description.

#### Scenario: Three feature cards are displayed
- **WHEN** the landing page is rendered
- **THEN** exactly three feature cards SHALL be visible
- **AND** the cards SHALL display: "Shared Backlog", "Rate & Compare", and "Discover Together"

#### Scenario: Feature cards use card component
- **WHEN** the feature highlights section is rendered
- **THEN** each feature card SHALL be rendered using the `BloomCardComponent`

### Requirement: Responsive layout
The landing page SHALL be responsive across mobile and desktop viewports. The hero title SHALL scale down on smaller screens and the feature grid SHALL reflow to a single column on narrow viewports.

#### Scenario: Desktop layout
- **WHEN** the viewport width is 641px or greater
- **THEN** the feature cards SHALL display in a multi-column grid layout
- **AND** the hero title SHALL use the full display size

#### Scenario: Mobile layout
- **WHEN** the viewport width is 640px or less
- **THEN** the feature cards SHALL stack in a single column
- **AND** the hero title SHALL use a reduced font size

### Requirement: Design system compliance
The landing page SHALL use design tokens for all visual properties. No hardcoded color, spacing, radius, or typography values are permitted.

#### Scenario: Tokens used for styling
- **WHEN** a developer inspects the landing page SCSS
- **THEN** all color values SHALL reference `--bloom-*` CSS custom properties
- **AND** all spacing values SHALL reference design token variables
- **AND** all border-radius values SHALL reference `--bloom-radius-*` tokens
