## 1. Dark Mode Token Overrides

- [x] 1.1 Add `html[data-theme="dark"]` block to `_tokens.scss` redefining all semantic surface tokens (`--bloom-surface-base`, `--bloom-surface-raised`, `--bloom-surface-overlay`, `--bloom-surface-sunken`, `--bloom-surface-tinted`, `--bloom-surface-body`) using deep purple neutral values
- [x] 1.2 Add dark-mode overrides for all text tokens (`--bloom-text-primary`, `--bloom-text-secondary`, `--bloom-text-tertiary`, `--bloom-text-disabled`, `--bloom-text-inverse`, `--bloom-text-link`, `--bloom-text-link-hover`)
- [x] 1.3 Add dark-mode overrides for border tokens (`--bloom-border-default`, `--bloom-border-strong`, `--bloom-border-focus`, `--bloom-border-error`)
- [x] 1.4 Add dark-mode overrides for gradient presets (`--bloom-gradient-surface`, `--bloom-gradient-kawaii`) that work on dark backgrounds
- [x] 1.5 Add dark-mode overrides for shadow tokens (reduce opacity or shift to darker tones that work on dark surfaces)
- [x] 1.6 Add dark-mode overrides for status background tokens (`--bloom-success-bg`, `--bloom-warning-bg`, `--bloom-error-bg`, `--bloom-info-bg`) using dark-appropriate tints

## 2. Semantic Token Audit & Fix

- [x] 2.1 Migrate ghost button hover in `bloom-button.scss` from hard-coded `--bloom-pink-50`/`--bloom-pink-200` to semantic tokens
- [x] 2.2 Migrate scrollbar track/thumb colors in `_base.scss` from palette tokens to semantic tokens
- [x] 2.3 Migrate `::selection` / `::-moz-selection` colors in `_base.scss` from palette tokens to semantic tokens
- [x] 2.4 Audit remaining `bloom-*` component SCSS files for any direct palette references used as surface/background colors and migrate to semantic tokens

## 3. ThemeService

- [x] 3.1 Create `core/theme/theme.service.ts` with `theme` WritableSignal, `toggle()`, `setTheme()`, localStorage read/write (wrapped in try-catch), and `prefers-color-scheme` matchMedia listener
- [x] 3.2 Provide `ThemeService` in root and initialize it on app startup (call from `app.config.ts` or shell component constructor)

## 4. Flash Prevention

- [x] 4.1 Add synchronous inline `<script>` to `index.html` that reads `localStorage('bloom-theme')` and sets `data-theme` on `<html>` before first paint

## 5. Theme Toggle Component

- [x] 5.1 Create `shared/ui/theme-toggle/bloom-theme-toggle.component.ts` — standalone component, injects `ThemeService`, displays sun/moon icon based on current theme, updates `aria-label` dynamically
- [x] 5.2 Create `shared/ui/theme-toggle/bloom-theme-toggle.scss` — BEM styling following existing bloom-* component conventions, kawaii hover animation, reduced-motion support
- [x] 5.3 Export `BloomThemeToggleComponent` from `shared/ui/index.ts` barrel

## 6. Shell Integration

- [x] 6.1 Add `bloom-theme-toggle` to the shell layout navigation bar (`shell-layout` component)

## 7. Verification

- [x] 7.1 Visually verify all existing bloom-* components render correctly in both light and dark themes (button variants, card, input, badge, avatar)
- [x] 7.2 Verify theme persists across page reload via localStorage
- [x] 7.3 Verify first-visit behavior respects OS `prefers-color-scheme`
- [x] 7.4 Verify no flash of wrong theme on page load for returning users
- [x] 7.5 Verify keyboard accessibility of the theme toggle (Enter/Space)
