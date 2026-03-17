## Why

BloomWatch currently has a single light pastel theme hard-coded into `:root`. Users need the ability to switch between light and dark modes for comfortable use in different lighting conditions. The existing design token architecture (semantic CSS custom properties referencing palette values) is already structured for theming — we just need dark-mode overrides and a toggle mechanism.

## What Changes

- Define a dark-mode palette override for all semantic tokens (surface, text, border, gradient, shadow, status backgrounds) applied via a `[data-theme="dark"]` attribute on `<html>`
- Create an Angular `ThemeService` that manages the active theme, persists the choice to `localStorage`, and respects `prefers-color-scheme` as the initial default
- Create a `bloom-theme-toggle` UI component for switching themes, accessible from the navigation shell
- Audit existing `bloom-*` components to ensure they use semantic tokens (not hard-coded palette values) so both themes render correctly
- Update `_base.scss` scrollbar and selection colors to use semantic tokens for theme-awareness

## Capabilities

### New Capabilities

- `theme-switching`: CSS custom property dark-mode overrides, Angular ThemeService (toggle, persist, system-preference detection), and bloom-theme-toggle UI component

### Modified Capabilities

_(none — no existing spec-level requirements are changing)_

## Impact

- **SCSS tokens** (`_tokens.scss`): Adds a `[data-theme="dark"]` block redefining semantic tokens; raw palette values stay unchanged
- **Base styles** (`_base.scss`): Scrollbar and selection colors switch to semantic tokens
- **Component styles**: Ghost button hover and any hard-coded light-mode references need semantic token migration
- **Angular core**: New `ThemeService` in `core/theme/`
- **Shared UI**: New `bloom-theme-toggle` component in `shared/ui/theme-toggle/`
- **Shell layout**: Toggle integrated into the navigation bar
- **No backend changes** — entirely frontend, UI-only scope
- **No breaking changes** — additive dark-mode layer; light mode remains default
