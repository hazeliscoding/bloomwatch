## ADDED Requirements

### Requirement: Dark mode token overrides
The system SHALL define a complete set of dark-mode semantic token overrides applied via `html[data-theme="dark"]`. The overrides SHALL cover all surface, text, border, gradient, shadow, and status background tokens. Raw palette tokens (`--bloom-pink-*`, `--bloom-blue-*`, etc.) SHALL NOT change between themes.

#### Scenario: Dark theme surfaces use deep purple palette
- **WHEN** the `data-theme` attribute on `<html>` is set to `"dark"`
- **THEN** `--bloom-surface-base` SHALL resolve to a value near `--bloom-neutral-900`, `--bloom-surface-body` SHALL resolve to a value near `--bloom-neutral-950`, and all surface tokens SHALL use dark neutral values

#### Scenario: Dark theme text is light on dark
- **WHEN** the `data-theme` attribute on `<html>` is set to `"dark"`
- **THEN** `--bloom-text-primary` SHALL resolve to a light neutral value (near `--bloom-neutral-50`), and `--bloom-text-secondary`/`--bloom-text-tertiary` SHALL use progressively dimmer light values

#### Scenario: Light theme remains default
- **WHEN** no `data-theme` attribute is present on `<html>` OR `data-theme` is `"light"`
- **THEN** all semantic tokens SHALL resolve to the current light-mode values defined in `:root`

### Requirement: Theme service manages active theme
The system SHALL provide an Angular `ThemeService` that exposes the current theme as a signal and allows toggling between light and dark modes.

#### Scenario: First visit with no stored preference and OS prefers dark
- **WHEN** `localStorage` has no `bloom-theme` key AND the OS reports `prefers-color-scheme: dark`
- **THEN** the service SHALL initialize with `'dark'` and set `data-theme="dark"` on `<html>`

#### Scenario: First visit with no stored preference and OS prefers light
- **WHEN** `localStorage` has no `bloom-theme` key AND the OS reports `prefers-color-scheme: light`
- **THEN** the service SHALL initialize with `'light'` and NOT set a `data-theme` attribute (or set it to `"light"`)

#### Scenario: Stored preference overrides OS setting
- **WHEN** `localStorage` contains `bloom-theme` with value `"dark"` AND the OS reports `prefers-color-scheme: light`
- **THEN** the service SHALL initialize with `'dark'` regardless of OS preference

#### Scenario: Toggle switches theme
- **WHEN** the current theme is `'light'` AND `toggle()` is called
- **THEN** the theme SHALL change to `'dark'`, the `data-theme` attribute SHALL update to `"dark"`, and `localStorage` key `bloom-theme` SHALL be set to `"dark"`

#### Scenario: OS preference changes while no stored preference exists
- **WHEN** no `bloom-theme` key exists in `localStorage` AND the OS switches from light to dark
- **THEN** the service SHALL update the active theme to `'dark'` in real time

#### Scenario: localStorage unavailable
- **WHEN** `localStorage` is not accessible (e.g., private browsing restrictions)
- **THEN** the service SHALL fall back to OS preference detection and default to `'light'` if OS preference is unavailable, without throwing errors

### Requirement: Theme toggle UI component
The system SHALL provide a `bloom-theme-toggle` standalone Angular component that allows users to switch between light and dark themes.

#### Scenario: Toggle displays current theme state
- **WHEN** the active theme is `'dark'`
- **THEN** the toggle SHALL display a sun icon (indicating "switch to light") and its `aria-label` SHALL be `"Switch to light mode"`

#### Scenario: Toggle displays light state
- **WHEN** the active theme is `'light'`
- **THEN** the toggle SHALL display a moon icon (indicating "switch to dark") and its `aria-label` SHALL be `"Switch to dark mode"`

#### Scenario: Clicking the toggle switches theme
- **WHEN** the user clicks the theme toggle
- **THEN** the theme SHALL switch immediately, the icon and `aria-label` SHALL update, and the change SHALL persist to `localStorage`

#### Scenario: Keyboard accessibility
- **WHEN** the toggle has keyboard focus AND the user presses Enter or Space
- **THEN** the toggle SHALL activate identically to a click

### Requirement: Theme toggle accessible from navigation
The system SHALL integrate the `bloom-theme-toggle` component into the application shell layout so it is accessible from any page.

#### Scenario: Toggle visible in navigation bar
- **WHEN** the application loads on any route
- **THEN** the theme toggle SHALL be visible in the top navigation bar

### Requirement: No flash of wrong theme on load
The system SHALL prevent a visible flash of the incorrect theme during page load.

#### Scenario: Returning dark-mode user sees no flash
- **WHEN** a user with `bloom-theme: "dark"` in `localStorage` loads the page
- **THEN** the `data-theme="dark"` attribute SHALL be set synchronously before the first paint, preventing any flash of light theme

### Requirement: Existing components render correctly in both themes
All existing `bloom-*` components SHALL render with correct contrast and readability in both light and dark themes. Components SHALL use semantic tokens for any color that changes between themes.

#### Scenario: Ghost button hover adapts to theme
- **WHEN** a ghost-variant `bloom-button` is hovered in dark mode
- **THEN** the hover background and border SHALL use semantic token values appropriate for the dark theme, not hard-coded light palette values

#### Scenario: Scrollbar colors adapt to theme
- **WHEN** the active theme is `'dark'`
- **THEN** the custom scrollbar track and thumb colors SHALL reflect dark-mode appropriate values

#### Scenario: Text selection colors adapt to theme
- **WHEN** text is selected in dark mode
- **THEN** the selection highlight SHALL use colors appropriate for a dark background with sufficient contrast
