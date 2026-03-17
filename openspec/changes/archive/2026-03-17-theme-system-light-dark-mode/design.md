## Context

BloomWatch's frontend uses a kawaii/Y2K design system built on CSS custom properties defined in `_tokens.scss`. All tokens live in a single `:root` block — semantic tokens (surface, text, border) reference raw palette values. Components consume semantic tokens, though some (ghost button hover, scrollbar, selection) reference palette values directly.

The existing architecture is well-suited for theming: semantic tokens act as an indirection layer. Adding a dark theme means overriding ~30 semantic tokens under a `[data-theme="dark"]` selector, plus an Angular service to manage the toggle and persistence.

**Current state:**
- `_tokens.scss`: ~345 lines, all in `:root`. Raw palette (pink, blue, lilac, lime, peach, yellow, neutral) + semantic tokens (surface, text, border, gradient, shadow, status)
- `_base.scss`: Body references `--bloom-surface-body` and `--bloom-gradient-surface` — already semantic. Scrollbar and selection colors reference palette tokens directly.
- Components (`bloom-button`, `bloom-card`, `bloom-input`, `bloom-badge`, `bloom-avatar`): Mostly use semantic tokens. Ghost button hover uses `--bloom-pink-50`/`--bloom-pink-200` directly.
- No theme infrastructure exists today — no service, no toggle, no `prefers-color-scheme` detection.

## Goals / Non-Goals

**Goals:**
- Define a dark-mode token set that preserves the kawaii/Y2K personality (deep purples, muted neons, soft glows — not generic gray dark mode)
- Let users toggle themes via a UI control in the navigation shell
- Persist theme choice to `localStorage`; respect `prefers-color-scheme` as default for first-time visitors
- Ensure all existing components render correctly in both themes without per-component overrides

**Non-Goals:**
- Custom/user-created themes or accent color picker — single light + single dark for now
- Server-side theme persistence (no API changes)
- Animated theme transitions (cross-fade between themes) — keep it a simple instant swap
- Redesigning existing components — only fix direct palette references that break in dark mode

## Decisions

### 1. Theme application mechanism: `data-theme` attribute on `<html>`

Override semantic tokens using `html[data-theme="dark"] { ... }` in `_tokens.scss`.

**Why over alternatives:**
- **vs. separate CSS file**: A single file keeps tokens co-located and diffable; no runtime CSS loading needed
- **vs. CSS class (`.dark`)**: Data attributes convey state rather than styling intent; clearer semantics and avoids class name collisions
- **vs. `prefers-color-scheme` media query only**: Users need explicit override control beyond OS setting

### 2. Only override semantic tokens, not raw palette

The dark theme redefines `--bloom-surface-*`, `--bloom-text-*`, `--bloom-border-*`, `--bloom-gradient-*`, `--bloom-shadow-*`, and `--bloom-status-*-bg` tokens. Raw palette values (`--bloom-pink-500`, etc.) stay the same in both themes.

**Why:** Components that correctly use semantic tokens get dark mode for free. Raw palette values represent the brand palette and should be consistent regardless of theme. Any component referencing raw palette tokens for surfaces/backgrounds is a bug to fix.

### 3. ThemeService in `core/theme/`

An Angular injectable service using signals:
- `theme: WritableSignal<'light' | 'dark'>` — current active theme
- `toggle()` — switches theme
- `setTheme(theme)` — explicit setter
- On init: reads `localStorage` key `bloom-theme`, falls back to `prefers-color-scheme`, defaults to `'light'`
- On change: sets `data-theme` attribute on `document.documentElement` and writes to `localStorage`
- Listens to `prefers-color-scheme` changes via `matchMedia` so if user is on "system" and OS changes, theme updates live

**Why signals over BehaviorSubject:** Aligns with Angular 19 signal-based patterns already used in the component library. Lighter weight, no subscription management.

### 4. bloom-theme-toggle component in `shared/ui/theme-toggle/`

A simple toggle button (sun/moon icons) following existing bloom-* conventions:
- Standalone Angular component with `bloom-theme-toggle` selector
- Injects `ThemeService`; displays current state
- Uses accessible labeling (`aria-label` updates with current state)
- Follows existing BEM SCSS and `input()`/`output()` signal patterns

### 5. Dark palette direction: deep purple surfaces, muted neon accents

Not a generic gray dark mode. The dark theme surfaces use deep purples from the neutral palette (`--bloom-neutral-900` → `--bloom-neutral-950`) to maintain the kawaii/Y2K vibe. Accent/status colors are slightly desaturated to reduce eye strain on dark backgrounds. Gel button gradients and glow shadows become more prominent against dark surfaces.

## Risks / Trade-offs

- **Hard-coded palette references in components** → Audit and fix before shipping. Known: ghost button hover (`pink-50`, `pink-200`), scrollbar colors, selection colors, `pre` background. Low effort — fewer than 10 occurrences.
- **Third-party content or images may not adapt** → Non-goal for MVP. Images/embeds render as-is in both themes. Can revisit with `filter: invert()` or themed image variants later.
- **`localStorage` not available (private browsing)** → ThemeService wraps reads/writes in try-catch; gracefully falls back to system preference or light default.
- **Flash of wrong theme on page load (FOWT)** → Mitigated by adding a synchronous inline `<script>` in `index.html` that reads `localStorage` and sets `data-theme` before Angular bootstraps. This prevents a visible flash.
