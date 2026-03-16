# BloomWatch UI/UX Doctrine

**Version**: 1.0
**Last Updated**: 2026-03-16
**Status**: Authoritative -- all BloomWatch UI development must conform to this document.

---

## Table of Contents

1. [Design Philosophy](#1-design-philosophy)
2. [Design Tokens Reference](#2-design-tokens-reference)
3. [Component Usage Guidelines](#3-component-usage-guidelines)
4. [Layout System](#4-layout-system)
5. [Interaction & Motion Design](#5-interaction--motion-design)
6. [Accessibility Standards](#6-accessibility-standards)
7. [CSS Architecture Rules](#7-css-architecture-rules)
8. [Angular Component Patterns](#8-angular-component-patterns)
9. [Common UI Patterns](#9-common-ui-patterns)
10. [Anti-Patterns](#10-anti-patterns-what-not-to-do)

---

## 1. Design Philosophy

### 1.1 The Kawaii/Y2K Identity

BloomWatch draws from two overlapping aesthetic traditions:

**Kawaii** -- Rounded shapes, soft pastels, playful micro-interactions, and a sense of warmth. Everything should feel approachable and gentle. The rounded corners on our components (radius-lg through radius-2xl), the pastel color palette (pink-50 through pink-200 surface tints), and the Quicksand/Nunito font pairing all serve this goal.

**Y2K** -- Gel button effects, glossy shine overlays, gradient borders, sparkle animations, and saturated accent colors. This is visible in our gel-shine `::before` pseudo-elements on buttons, our `--bloom-gradient-gel-*` tokens, and the animated sparkle borders on highlighted cards.

**What this identity means in practice:**
- Components have generous border-radius values (minimum `--bloom-radius-lg` / 16px for interactive elements).
- Surfaces use soft shadows with colored tints rather than harsh black shadows.
- Primary interactions carry a "gel" quality -- glossy, dimensional, tactile.
- Animations use spring/bounce easing (`--bloom-ease-bounce`, `--bloom-ease-spring`) to feel organic rather than mechanical.
- Color choices skew toward pink, lilac, and soft blue. These are our emotional anchors.

**What this identity does NOT mean:**
- It does not mean cluttered. Every decorative element must serve a purpose -- guiding attention, communicating state, or providing feedback.
- It does not mean childish. The aesthetic is playful and inviting, not juvenile. Avoid excessive emoji, cartoon illustrations as primary UI elements, or baby-talk copy.
- It does not mean sacrificing usability. Every kawaii flourish exists alongside -- never instead of -- clear information hierarchy, readable text, and accessible interactions.

### 1.2 Emotional Goals

| Goal | Manifestation |
|------|--------------|
| **Playful** | Bounce/spring easing on hover, gel button shine, sparkle borders, float animations |
| **Warm** | Pink-tinted surfaces (`--bloom-surface-tinted`, `--bloom-surface-body`), pastel gradients, rounded everything |
| **Inviting** | Low-contrast soft shadows, generous spacing, friendly rounded fonts (Quicksand headings, Nunito body) |
| **Trustworthy** | Consistent component behavior, clear status colors, predictable layout patterns |
| **Focused** | Clean information hierarchy, restrained use of color, whitespace as a first-class design tool |

### 1.3 The Balance Principle

Every UI decision in BloomWatch must pass this test: **does the aesthetic choice improve or impede the user's ability to accomplish their task?**

- A sparkle border on a highlighted WatchSpace card draws attention to the most relevant item. This improves task flow. Keep it.
- A bouncing animation on every list item would distract from reading. This impedes task flow. Remove it.
- A gel button with hover-lift gives tactile feedback that an element is interactive. This improves clarity. Keep it.
- A rainbow gradient on body text would reduce readability. This impedes comprehension. Never do it.

When in doubt, choose clarity over flair.

---

## 2. Design Tokens Reference

All visual values in BloomWatch are defined as CSS custom properties in `_tokens.scss`. Using raw values (hex colors, pixel sizes, millisecond durations) directly in component styles is prohibited. Every value must reference a token.

### 2.1 Color System

#### Palette Colors

The system provides six color ramps, each with shades from 50 (lightest) to 900 (darkest):

| Palette | Token prefix | Role |
|---------|-------------|------|
| **Pink** | `--bloom-pink-*` | Primary brand color. Hot pink. Used for primary actions, links, brand identity, focus rings. |
| **Blue** | `--bloom-blue-*` | Secondary brand color. Electric blue. Used for secondary actions, informational states. |
| **Lilac** | `--bloom-lilac-*` | Accent color. Purple. Used for accent actions, decorative elements, gradient blending. |
| **Lime** | `--bloom-lime-*` | Success/positive accent. Used for "watching," "completed," positive states. |
| **Peach** | `--bloom-peach-*` | Warm accent. Used sparingly for warm highlights, sunset gradients. |
| **Yellow** | `--bloom-yellow-*` | Caution/highlight accent. Used for warnings, "on hold" states. |
| **Neutral** | `--bloom-neutral-*` | Gray ramp with a subtle lilac undertone (not pure gray). Shades 0 (white) through 950 (near-black). |

#### When to Use Which Color

**Primary actions** (submit, confirm, main CTA): `--bloom-pink-*` shades, or `--bloom-gradient-gel-pink`.
**Secondary actions** (alternative actions, less emphasis): `--bloom-blue-*` shades, or `--bloom-gradient-gel-blue`.
**Accent/tertiary actions** (special features, optional actions): `--bloom-lilac-*` shades, or `--bloom-gradient-gel-lilac`.
**Destructive actions** (delete, remove, cancel): Use the `danger` button variant. Never use pink for destructive actions -- pink is a positive color in this system.
**Neutral/passive actions** (dismiss, close, back): `ghost` button variant, neutral palette.

#### Semantic Color Aliases

Always prefer semantic tokens over raw palette tokens when the purpose is semantic:

```scss
// CORRECT -- semantic intent is clear
color: var(--bloom-primary);
background-color: var(--bloom-surface-raised);
border-color: var(--bloom-border-focus);

// WRONG -- raw palette, hides intent
color: var(--bloom-pink-500);
background-color: var(--bloom-neutral-50);
border-color: var(--bloom-pink-400);
```

When you need a specific shade for a visual effect (e.g., a gradient stop, a tinted background), using the palette token directly is acceptable. But for meaning-bearing uses (error, surface, border, text), use the semantic alias.

**Semantic tokens available:**

| Category | Tokens |
|----------|--------|
| Brand | `--bloom-primary`, `--bloom-primary-light`, `--bloom-primary-dark` |
| Brand | `--bloom-secondary`, `--bloom-secondary-light`, `--bloom-secondary-dark` |
| Brand | `--bloom-accent`, `--bloom-accent-light`, `--bloom-accent-dark` |
| Status | `--bloom-success`, `--bloom-success-light`, `--bloom-success-dark`, `--bloom-success-bg` |
| Status | `--bloom-warning`, `--bloom-warning-light`, `--bloom-warning-dark`, `--bloom-warning-bg` |
| Status | `--bloom-error`, `--bloom-error-light`, `--bloom-error-dark`, `--bloom-error-bg` |
| Status | `--bloom-info`, `--bloom-info-light`, `--bloom-info-dark`, `--bloom-info-bg` |
| Surfaces | `--bloom-surface-base`, `--bloom-surface-raised`, `--bloom-surface-overlay`, `--bloom-surface-sunken`, `--bloom-surface-tinted`, `--bloom-surface-body` |
| Text | `--bloom-text-primary`, `--bloom-text-secondary`, `--bloom-text-tertiary`, `--bloom-text-disabled`, `--bloom-text-inverse`, `--bloom-text-link`, `--bloom-text-link-hover` |
| Borders | `--bloom-border-default`, `--bloom-border-strong`, `--bloom-border-focus`, `--bloom-border-error` |

#### Gradient Presets

Use the pre-defined gradient tokens. Do not invent new gradients without adding them as tokens first.

| Token | Use Case |
|-------|----------|
| `--bloom-gradient-primary` | Pink gradient (135deg), CTA backgrounds |
| `--bloom-gradient-secondary` | Blue gradient (135deg), secondary CTA backgrounds |
| `--bloom-gradient-accent` | Lilac gradient (135deg), accent highlights |
| `--bloom-gradient-kawaii` | Pink-to-lilac-to-blue (135deg), brand watermark, decorative elements, gradient text |
| `--bloom-gradient-sunset` | Pink-to-peach (135deg), warm decorative elements |
| `--bloom-gradient-ocean` | Lilac-to-blue (135deg), cool decorative elements |
| `--bloom-gradient-surface` | Vertical pink-to-lilac-to-blue, used on `body` background |
| `--bloom-gradient-gel-pink` | Vertical gel button gradient (pink), used in `.bloom-btn--primary` |
| `--bloom-gradient-gel-blue` | Vertical gel button gradient (blue), used in `.bloom-btn--secondary` |
| `--bloom-gradient-gel-lilac` | Vertical gel button gradient (lilac), used in `.bloom-btn--accent` |

### 2.2 Typography

#### Font Families

| Token | Font | Usage |
|-------|------|-------|
| `--bloom-font-family-display` | Quicksand (fallback: Nunito, system-ui) | Headings, navigation labels, button labels, badge labels, input labels. Anything that is a title or a control label. |
| `--bloom-font-family-body` | Nunito (fallback: system-ui) | Body text, paragraphs, descriptions, form field values, list items. Anything that is running text. |
| `--bloom-font-family-mono` | JetBrains Mono (fallback: Fira Code, ui-monospace) | Code blocks, technical identifiers, debug output. |

**Rule**: Never mix display and body fonts within a single text element. Headings get display font. Body text gets body font. There are no exceptions.

#### Font Size Scale

The scale follows an approximate 1.25 ratio. Use these tokens exclusively:

| Token | Size | Usage guidance |
|-------|------|----------------|
| `--bloom-text-xs` | 0.75rem (12px) | Hint text, error messages, fine print, timestamps |
| `--bloom-text-sm` | 0.875rem (14px) | Secondary labels, input labels (sm/md), navigation links, badge text, small button text |
| `--bloom-text-base` | 1rem (16px) | Body text, input field values (md), medium button text |
| `--bloom-text-md` | 1.125rem (18px) | Emphasized body text, large input values, large button text |
| `--bloom-text-lg` | 1.25rem (20px) | H4, subtitle text, brand name in nav |
| `--bloom-text-xl` | 1.5rem (24px) | H3, section subheadings |
| `--bloom-text-2xl` | 1.875rem (30px) | H2 (mobile), prominent section headings |
| `--bloom-text-3xl` | 2.25rem (36px) | H1 (mobile), H2 (desktop) |
| `--bloom-text-4xl` | 3rem (48px) | H1 (desktop), hero headings |

#### Font Weights

| Token | Weight | Usage |
|-------|--------|-------|
| `--bloom-font-light` | 300 | Decorative large text only. Never for body text. |
| `--bloom-font-regular` | 400 | Default body text weight |
| `--bloom-font-medium` | 500 | Slightly emphasized body text, error messages |
| `--bloom-font-semibold` | 600 | Input labels, navigation links, badge text, button text, `<strong>` |
| `--bloom-font-bold` | 700 | Headings (all levels), avatar initials |
| `--bloom-font-extrabold` | 800 | Brand logotype only (`BloomWatch` in the nav bar) |

#### Line Heights

| Token | Value | Usage |
|-------|-------|-------|
| `--bloom-leading-none` | 1 | Single-line elements only (badges, buttons, icons) |
| `--bloom-leading-tight` | 1.25 | Headings |
| `--bloom-leading-snug` | 1.375 | Multi-line headings, tight UI text |
| `--bloom-leading-normal` | 1.5 | Default body text |
| `--bloom-leading-relaxed` | 1.625 | Paragraph text (set in base styles for `<p>`) |
| `--bloom-leading-loose` | 2 | Large-spaced text blocks, rarely used |

### 2.3 Spacing System

BloomWatch uses a 4px base grid. All spacing values are multiples or simple fractions of this base.

| Token | Value | Common uses |
|-------|-------|-------------|
| `--bloom-space-0` | 0 | Reset |
| `--bloom-space-px` | 1px | Hairline borders |
| `--bloom-space-0-5` | 2px | Badge padding (sm), required asterisk spacing |
| `--bloom-space-1` | 4px | Icon gaps within buttons/badges, tight inline spacing |
| `--bloom-space-1-5` | 6px | Input label-to-field gap, button padding (sm vertical), badge dot size spacing |
| `--bloom-space-2` | 8px | Button icon gap, input prefix/suffix gap, nav brand icon gap, button padding (md vertical) |
| `--bloom-space-2-5` | 10px | Input field padding (md vertical) |
| `--bloom-space-3` | 12px | Button padding (lg vertical), nav link padding, card header-to-body gap, footer border-top gap |
| `--bloom-space-4` | 16px | Default page content padding (mobile), paragraph spacing, container inline padding (mobile) |
| `--bloom-space-5` | 20px | Card internal padding, button padding (md horizontal), input padding (lg horizontal) |
| `--bloom-space-6` | 24px | Shell content padding (mobile), container inline padding (tablet), section spacing |
| `--bloom-space-8` | 32px | Shell content padding (desktop), button padding (lg horizontal), major section gaps |
| `--bloom-space-10` | 40px | Shell content side padding (desktop lg), large gaps |
| `--bloom-space-12` | 48px | Large vertical rhythm between major sections |
| `--bloom-space-16` | 64px | Extra-large spacing, page-level vertical rhythm |
| `--bloom-space-20` | 80px | Hero sections, splash screens |
| `--bloom-space-24` | 96px | Maximum vertical spacing |

**Rule**: Use the spacing token closest to your needs. If you find yourself wanting a value that doesn't exist (e.g., 13px), round to the nearest token (12px = `--bloom-space-3`). Do not create ad-hoc values.

### 2.4 Border Radius

All interactive and container elements in BloomWatch use rounded corners. The scale:

| Token | Value | Usage |
|-------|-------|-------|
| `--bloom-radius-none` | 0 | Explicitly square elements only (extremely rare) |
| `--bloom-radius-sm` | 6px | Inline code blocks, focus outlines on links, small accent shapes |
| `--bloom-radius-md` | 10px | Skip link corners |
| `--bloom-radius-lg` | 16px | Small buttons (sm), small input wrappers (sm), mobile nav links, nav toggle button |
| `--bloom-radius-xl` | 20px | Medium buttons (md), medium inputs (md), nav links (desktop) |
| `--bloom-radius-2xl` | 24px | Large buttons (lg), large inputs (lg), cards (both variants) |
| `--bloom-radius-3xl` | 32px | Extra-large containers, hero sections |
| `--bloom-radius-pill` | 9999px | Badges, scrollbar tracks/thumbs, decorative pills, divider lines |
| `--bloom-radius-circle` | 50% | Avatars, status dots, spinner dots |

**Rule**: Border radius for a component must scale with its size. Small components get `--bloom-radius-lg`. Medium components get `--bloom-radius-xl`. Large components get `--bloom-radius-2xl`. This is already established in button and input size variants -- follow the same pattern for all new components.

### 2.5 Elevation / Shadows

Shadows in BloomWatch use our neutral-900 color as a base (`rgb(33 26 51 / ...)`) to maintain the lilac-undertone neutral palette. They are soft and diffused, never harsh.

| Token | Usage |
|-------|-------|
| `--bloom-shadow-xs` | Subtle depth: ghost button hover, input focus companion |
| `--bloom-shadow-sm` | Default card elevation, button resting state, avatar frame |
| `--bloom-shadow-md` | Hovered buttons, active cards, highlighted card resting state |
| `--bloom-shadow-lg` | Hovered cards, mobile nav dropdown, elevated overlays |
| `--bloom-shadow-xl` | Hovered highlighted cards, modals, top-level overlays |

**Colored glow shadows** (for special emphasis only):
| Token | Usage |
|-------|-------|
| `--bloom-shadow-glow-pink` | Primary CTA emphasis, active WatchSpace indicator |
| `--bloom-shadow-glow-blue` | Secondary emphasis, informational highlights |
| `--bloom-shadow-glow-lilac` | Accent emphasis, premium feature indicators |
| `--bloom-shadow-glow-lime` | Success/positive emphasis |

**Inner glow** (`--bloom-shadow-inner-glow`): Used exclusively for gel-effect components. Do not apply to flat surfaces.

**Rule**: A resting component should use `--bloom-shadow-sm` or `--bloom-shadow-md`. Hovered state escalates by one level. Active/pressed state de-escalates. Do not jump more than two levels in a single interaction.

### 2.6 Animation Timing

| Token | Duration | Usage |
|-------|----------|-------|
| `--bloom-duration-instant` | 75ms | Immediate feedback: opacity changes, color swaps |
| `--bloom-duration-fast` | 150ms | Button transitions, input border changes, focus states |
| `--bloom-duration-normal` | 250ms | Card hover transitions, nav link transitions, fade-in animations |
| `--bloom-duration-slow` | 400ms | Slide-in animations, page content entrance, staggered reveals |
| `--bloom-duration-slower` | 600ms | Complex orchestrated animations (used sparingly) |

**Easing functions**:

| Token | Curve | Usage |
|-------|-------|-------|
| `--bloom-ease-default` | `cubic-bezier(0.4, 0, 0.2, 1)` | General purpose. Use for most transitions. |
| `--bloom-ease-in` | `cubic-bezier(0.4, 0, 1, 1)` | Elements leaving the screen. |
| `--bloom-ease-out` | `cubic-bezier(0, 0, 0.2, 1)` | Elements entering the screen. Fade-in, slide-in. |
| `--bloom-ease-in-out` | `cubic-bezier(0.4, 0, 0.2, 1)` | Symmetric transitions. |
| `--bloom-ease-bounce` | `cubic-bezier(0.34, 1.56, 0.64, 1)` | Kawaii button hover-lift, playful micro-interactions. Overshoots slightly. |
| `--bloom-ease-spring` | `cubic-bezier(0.22, 1.4, 0.36, 1)` | Card hover, nav brand icon rotation. More pronounced overshoot than bounce. |

**Composite shorthand tokens** (duration + easing):

| Token | Usage |
|-------|-------|
| `--bloom-transition-fast` | Button state changes, input focus rings |
| `--bloom-transition-normal` | Card hover, general component transitions |
| `--bloom-transition-slow` | Page-level animations, complex reveals |
| `--bloom-transition-bounce` | Button vertical lift on hover |
| `--bloom-transition-spring` | Card lift on hover, playful element transitions |

### 2.7 Z-Index Scale

Never use raw z-index numbers. Always use the token scale:

| Token | Value | Usage |
|-------|-------|-------|
| `--bloom-z-behind` | -1 | Background decorative elements, sparkle border pseudo-elements |
| `--bloom-z-base` | 0 | Default stacking |
| `--bloom-z-raised` | 10 | Cards, raised surfaces |
| `--bloom-z-dropdown` | 100 | Dropdown menus, autocomplete lists |
| `--bloom-z-sticky` | 200 | Sticky navigation bar (used by `shell-nav`) |
| `--bloom-z-overlay` | 300 | Overlay backgrounds, drawer backdrops |
| `--bloom-z-modal` | 400 | Modal dialogs |
| `--bloom-z-popover` | 500 | Popovers, floating elements |
| `--bloom-z-toast` | 600 | Toast notifications |
| `--bloom-z-tooltip` | 700 | Tooltips, skip-link |

---

## 3. Component Usage Guidelines

### 3.1 BloomButton (`<bloom-button>`)

**Selector**: `bloom-button`
**Import**: `BloomButtonComponent` from `@shared/ui`

#### API

| Input | Type | Default | Description |
|-------|------|---------|-------------|
| `variant` | `'primary' \| 'secondary' \| 'accent' \| 'ghost' \| 'danger'` | `'primary'` | Visual style |
| `size` | `'sm' \| 'md' \| 'lg'` | `'md'` | Size scale |
| `type` | `'button' \| 'submit' \| 'reset'` | `'button'` | HTML button type |
| `disabled` | `boolean` | `false` | Disables interaction |
| `loading` | `boolean` | `false` | Shows spinner, disables interaction |
| `fullWidth` | `boolean` | `false` | Stretches to container width |

| Output | Type | Description |
|--------|------|-------------|
| `clicked` | `MouseEvent` | Emitted on click (suppressed when disabled/loading) |

**Content projection slots**:
- Default: Button label text
- `[bloomButtonIconLeft]`: Icon before label
- `[bloomButtonIconRight]`: Icon after label

#### When to Use Each Variant

| Variant | Use When | Example |
|---------|----------|---------|
| `primary` | The single most important action on screen. One per visible section maximum. | "Create WatchSpace", "Save Changes", "Log In" |
| `secondary` | Important but not the primary action. Can appear alongside primary. | "View Profile", "Sync AniList", "Search" |
| `accent` | Special, optional, or exploratory actions. Used to add visual variety. | "Explore", "Try Premium", feature-specific actions |
| `ghost` | Low-emphasis actions, navigation-like actions, cancel/dismiss. | "View More", "Cancel", "Back", "Invite", toolbar actions |
| `danger` | Destructive, irreversible, or high-consequence actions. | "Delete WatchSpace", "Remove Member", "Disconnect Account" |

**Never**:
- Use `primary` for destructive actions. Use `danger`.
- Use `danger` for non-destructive actions. The red gel gradient must always mean "this will remove or destroy something."
- Place two `primary` buttons adjacent to each other. If two actions are equally important, make one `secondary`.
- Use `ghost` for the main CTA. It is too low-emphasis.

#### Size Selection

| Size | When to Use |
|------|-------------|
| `sm` | Inside cards, inline with text, table rows, secondary actions in tight spaces. Min-height 2rem. |
| `md` | Default. Forms, standalone CTAs, dialog actions. Min-height 2.5rem. |
| `lg` | Hero CTAs, full-width mobile actions, prominent standalone actions. Min-height 3rem. |

#### Icon Usage

Icons are projected into named slots. Always pair icons with text labels. Icon-only buttons are not yet supported by this component. When an icon-only button is needed, it should be built as a separate component.

```html
<!-- Icon on the left -->
<bloom-button variant="primary">
  <span bloomButtonIconLeft>ICON</span>
  Like
</bloom-button>

<!-- Icon on the right -->
<bloom-button variant="secondary">
  Add
  <span bloomButtonIconRight>ICON</span>
</bloom-button>
```

#### Loading State

Set `[loading]="true"` to show a three-dot bounce spinner and hide the label/icons. The button retains its dimensions. The `aria-busy="true"` attribute is set automatically.

```html
<bloom-button [loading]="isSubmitting()" (clicked)="onSubmit()">
  Save Changes
</bloom-button>
```

#### Accessibility

- The component sets `aria-disabled` and `aria-busy` automatically.
- The inner `<button>` element receives the `type` attribute -- always set `type="submit"` for form submission buttons.
- Focus is indicated by a 2px `--bloom-pink-400` outline with 2px offset.
- Click events are suppressed when disabled or loading -- no additional guard logic needed in the parent.

---

### 3.2 BloomCard (`<bloom-card>`)

**Selector**: `bloom-card`
**Import**: `BloomCardComponent` from `@shared/ui`

#### API

| Input | Type | Default | Description |
|-------|------|---------|-------------|
| `variant` | `'default' \| 'highlighted'` | `'default'` | Visual style |
| `hoverable` | `boolean` | `true` | Enables hover-lift effect |
| `ariaLabel` | `string \| undefined` | `undefined` | Accessible label for the card article element |

**Content projection slots**:
- `[bloomCardHeader]`: Card header area (receives decorative kawaii gradient accent line)
- Default: Card body content
- `[bloomCardFooter]`: Card footer area (receives top border separator when non-empty)

#### When to Use Each Variant

| Variant | Use When |
|---------|----------|
| `default` | Standard content containers. Dashboard widgets, list items, settings panels, search results. The majority of cards. |
| `highlighted` | A single card that needs emphasis. The user's primary WatchSpace, a featured anime, a premium feature, an active watch party. Use sparingly -- if everything is highlighted, nothing is. |

#### When NOT to Use a Card

- For full-page content sections. Cards are discrete, bounded containers. Use page-level layout with `--bloom-surface-base` backgrounds instead.
- For single-line items. Use a simple flex row or list item, not a card with padding overhead.
- For navigation elements. Cards are content containers, not links. If a card appears clickable, it should contain a button or link inside, not be wrapped in an `<a>` tag.

#### Composition Rules

The header slot receives a decorative kawaii gradient line (`--bloom-gradient-kawaii`) at the top of the card when populated. This is automatic. Do not add additional decorative borders to card headers.

The footer slot gains a `1px solid --bloom-border-default` top border and auto margin-top when populated. This pushes the footer to the bottom of a flex-column layout. Do not manually add borders to card footers.

Empty slots (`header`, `footer`) are hidden via `:empty { display: none }`. This means if you project nothing, there is no visual artifact.

```html
<!-- Full card with all slots -->
<bloom-card>
  <h3 bloomCardHeader>WatchSpace Name</h3>
  <p>Description text goes here in the default body slot.</p>
  <div bloomCardFooter>
    <bloom-button variant="primary" size="sm">Open</bloom-button>
    <bloom-button variant="ghost" size="sm">Invite</bloom-button>
  </div>
</bloom-card>

<!-- Minimal card (body only) -->
<bloom-card [hoverable]="false">
  <p>Simple content without header or footer.</p>
</bloom-card>
```

#### Accessibility

- The root element is an `<article>`. Set `ariaLabel` when the card's purpose is not obvious from its heading content.
- If the card is not interactive, set `[hoverable]="false"` to avoid implying interactivity.
- The sparkle-border on `highlighted` variant is `aria-hidden="true"`.

---

### 3.3 BloomInput (`<bloom-input>`)

**Selector**: `bloom-input`
**Import**: `BloomInputComponent` from `@shared/ui`

#### API

| Input | Type | Default | Description |
|-------|------|---------|-------------|
| `label` | `string` | `''` | Visible label text |
| `placeholder` | `string` | `''` | Placeholder text |
| `type` | `string` | `'text'` | HTML input type |
| `size` | `'sm' \| 'md' \| 'lg'` | `'md'` | Size scale |
| `disabled` | `boolean` | `false` | Disables the input |
| `readonly` | `boolean` | `false` | Makes the input read-only |
| `required` | `boolean` | `false` | Marks as required (shows pink asterisk) |
| `error` | `string` | `''` | Error message (triggers error state) |
| `hint` | `string` | `''` | Hint text (shown when no error) |
| `inputId` | `string` | auto-generated | HTML id for the input element |
| `autocomplete` | `string` | `'off'` | HTML autocomplete attribute |

| Output | Type | Description |
|--------|------|-------------|
| `valueChange` | `string` | Emitted on every input event |

**Content projection slots**:
- `[bloomInputPrefix]`: Icon/element before the input field
- `[bloomInputSuffix]`: Icon/element after the input field

**ControlValueAccessor**: This component implements `ControlValueAccessor`. It works with both template-driven and reactive forms.

#### When to Use

Use `bloom-input` for all single-line text inputs in the application. This includes text, email, password, search, url, tel, and number types.

Do not use it for:
- Multi-line text (build a `bloom-textarea` component following the same patterns).
- Select/dropdown inputs (build a `bloom-select` component).
- Checkboxes, radio buttons, toggles (these need their own components).

#### Size Selection

| Size | When to Use |
|------|-------------|
| `sm` | Inline search bars, filters, compact forms, within cards. Min-height 2rem. |
| `md` | Default. Login/register forms, settings forms, standard data entry. Min-height 2.75rem. |
| `lg` | Prominent search bars, hero-level input, onboarding forms. Min-height 3.25rem. |

#### Error and Hint Display

- When `error` is non-empty, the input enters error state: red border, red background tint, label turns red, error message appears with a shake animation, and `aria-invalid="true"` is set.
- When `hint` is provided and there is no error, hint text appears below the field.
- Error and hint are linked to the input via `aria-describedby`.
- Error messages use `role="alert"` for screen reader announcement.

```html
<!-- With validation -->
<bloom-input
  label="Email"
  type="email"
  placeholder="you@example.com"
  [required]="true"
  [error]="emailError()"
  hint="We'll never share your email"
  autocomplete="email"
/>

<!-- With prefix icon -->
<bloom-input label="Search" placeholder="Search anime..." size="sm">
  <span bloomInputPrefix>SEARCH_ICON</span>
</bloom-input>
```

#### Accessibility

- Labels are automatically linked to inputs via the `for`/`id` relationship.
- Required fields show a visual asterisk (hidden from screen readers with `aria-hidden="true"`) and set the HTML `required` attribute.
- Error messages are announced via `role="alert"`.
- The `aria-describedby` attribute chains to either the hint or error paragraph ID.
- Focus produces a pink ring shadow (`0 0 0 3px rgb(255 45 138 / 0.12)`).

---

### 3.4 BloomBadge (`<bloom-badge>`)

**Selector**: `bloom-badge`
**Import**: `BloomBadgeComponent` from `@shared/ui`

#### API

| Input | Type | Default | Description |
|-------|------|---------|-------------|
| `color` | `'pink' \| 'blue' \| 'green' \| 'lilac' \| 'yellow' \| 'neutral'` | `'pink'` | Color variant |
| `size` | `'sm' \| 'md'` | `'md'` | Size scale |
| `dot` | `boolean` | `false` | Shows animated status dot |
| `ariaLabel` | `string \| undefined` | `undefined` | Accessible label |

**Content projection**: Default slot for badge label text.

#### Color Semantics

Badges carry meaning through color. Be consistent:

| Color | Semantic Meaning in BloomWatch |
|-------|-------------------------------|
| `green` | Active/positive states: "Watching", "Online", "Completed", "Synced" |
| `pink` | Primary/brand states: "Completed" (alternative), genre tags (Romance), featured |
| `blue` | Informational: "Plan to Watch", genre tags (Action), sync status |
| `lilac` | Accent/special: genre tags (Fantasy), premium features |
| `yellow` | Caution/hold: "On Hold", "Paused", awaiting action |
| `neutral` | Inactive/dismissed: "Dropped", "Offline", archived |

#### Dot Indicator

The `dot` input adds a small animated pulsing circle before the label. Use it exclusively for **live status indicators** -- states that can change in real time (online/offline, watching/idle, sync status). Do not use dots on static labels like genre tags.

#### When NOT to Use a Badge

- For buttons. Badges are display-only; they do not have click handlers.
- For large blocks of text. Badges are for short labels (1-3 words).
- For counts/numbers alone. Use a counter element or number display, not a badge.

---

### 3.5 BloomAvatar (`<bloom-avatar>`)

**Selector**: `bloom-avatar`
**Import**: `BloomAvatarComponent` from `@shared/ui`

#### API

| Input | Type | Default | Description |
|-------|------|---------|-------------|
| `src` | `string` | `''` | Image URL |
| `alt` | `string` | `''` | Image alt text |
| `name` | `string` | `''` | User name (used for initials fallback and default alt) |
| `size` | `'xs' \| 'sm' \| 'md' \| 'lg'` | `'md'` | Size scale |
| `status` | `'online' \| 'offline' \| 'watching' \| 'none'` | `'none'` | Status indicator |
| `ariaLabel` | `string \| undefined` | `undefined` | Accessible label |

#### Size Selection

| Size | Dimensions | When to Use |
|------|-----------|-------------|
| `xs` | 1.5rem (24px) | Inside avatar stacks, inline mentions, compact lists |
| `sm` | 2rem (32px) | Navigation user display, avatar stacks, list items |
| `md` | 2.75rem (44px) | Card headers, comment authors, member lists |
| `lg` | 4rem (64px) | Profile displays, user detail views, settings pages |

#### Initials Fallback

When no `src` is provided, the component renders initials on a `--bloom-gradient-kawaii` background. The initials are derived from the `name` input:
- Two-word names: First letter of first word + first letter of last word (e.g., "Naruto Uzumaki" renders "NU").
- Single-word names: First two characters (e.g., "Sakura" renders "SA").
- No name: Renders "?".

Always provide `name` even when `src` is available, so the initials fallback works if the image fails to load.

#### Status Indicators

| Status | Visual | Use When |
|--------|--------|----------|
| `online` | Green dot with green glow | User is active in the app |
| `offline` | Gray dot (neutral-400) | User is not active |
| `watching` | Pink dot with animated pulse glow | User is currently in a watch session |
| `none` | No indicator | Status is irrelevant or unknown |

#### BloomAvatarStack (`<bloom-avatar-stack>`)

**Selector**: `bloom-avatar-stack`
**Import**: `BloomAvatarStackComponent` from `@shared/ui`

Wraps multiple `bloom-avatar` components in an overlapping row. Each avatar after the first has `-0.5rem` left margin. The decorative outer ring is removed in stack context to reduce visual noise.

```html
<bloom-avatar-stack ariaLabel="Watch party members">
  <bloom-avatar size="sm" name="User One" status="online" />
  <bloom-avatar size="sm" name="User Two" status="watching" />
  <bloom-avatar size="sm" name="User Three" status="offline" />
</bloom-avatar-stack>
```

**Rules**:
- Use `sm` or `xs` avatars in stacks. `md` and `lg` are too large for overlapping presentation.
- Keep stacks to 5 members maximum visually. For larger groups, show 4 avatars plus a "+N" overflow indicator (to be built as a new component).
- Always provide `ariaLabel` on the stack describing the group.

---

### 3.6 Creating New Components

When building a new shared UI component, follow these conventions established by the existing library:

**File structure**:
```
src/app/shared/ui/<component-name>/
  bloom-<component-name>.ts       # Component class
  bloom-<component-name>.scss     # Scoped styles
```

**Naming**:
- Selector: `bloom-<name>` (e.g., `bloom-toggle`, `bloom-dialog`)
- Class: `Bloom<Name>Component` (e.g., `BloomToggleComponent`)
- CSS root class: `.bloom-<name>` (e.g., `.bloom-toggle`)
- CSS child classes: `.bloom-<name>__<element>` (BEM, e.g., `.bloom-toggle__track`)
- CSS modifier classes: `.bloom-<name>--<modifier>` (BEM, e.g., `.bloom-toggle--checked`)

**Required characteristics**:
1. `standalone: true` -- all BloomWatch components are standalone.
2. Use `input()` signal function, never `@Input()` decorator.
3. Use `output()` signal function, never `@Output()` decorator.
4. Use `computed()` for derived state (e.g., CSS class maps).
5. Set `:host { display: inline-block; }` or `:host { display: block; }` in SCSS (inline-block for inline elements like buttons/badges, block for container elements like cards/inputs).
6. Include `@media (prefers-reduced-motion: reduce)` overrides for any animations.
7. Empty slot content must be hidden with `&:empty { display: none; }`.
8. Export the component and its types from `src/app/shared/ui/index.ts`.

---

## 4. Layout System

### 4.1 Two-Layout Architecture

BloomWatch uses two layout wrappers, selected by route configuration:

#### Shell Layout (`ShellLayout`)

**When to use**: All authenticated routes -- Dashboard, WatchSpaces, Settings, Showcase. Any page that needs the navigation bar.

**Structure**:
- Sticky top navigation bar with frosted-glass background (`backdrop-filter: blur(16px) saturate(1.8)`)
- Navigation inner constrained to `--bloom-container-xl` (1200px)
- Main content area (`<main class="shell-content">`) constrained to `--bloom-container-xl` with responsive padding
- Content entrance animation: `bloom-fade-in-up` with slow duration

**Content padding by breakpoint**:
| Breakpoint | Padding |
|-----------|---------|
| Mobile (< 768px) | `--bloom-space-6` (24px) all sides |
| Tablet (>= 768px) | `--bloom-space-8` (32px) all sides |
| Desktop (>= 1024px) | `--bloom-space-8` vertical, `--bloom-space-10` horizontal |

#### Minimal Layout (`MinimalLayout`)

**When to use**: Unauthenticated routes -- Login, Register. Any full-screen experience without the navigation chrome (onboarding flows, error pages).

**Structure**: Pure `<router-outlet />` with no wrapping elements. The rendered component is fully responsible for its own layout.

Pages rendered within MinimalLayout should center their content and use `--bloom-surface-body` (or the gradient background inherited from `body`) as their backdrop.

### 4.2 Page Structure Patterns

#### Standard Content Page (within Shell Layout)

Pages rendered inside `ShellLayout` receive the `shell-content` wrapper automatically. Your page component should NOT add its own max-width container unless it needs a narrower constraint.

```scss
// CORRECT -- page-specific structure
:host {
  display: block;
}

.page-header {
  margin-bottom: var(--bloom-space-8);
}

.page-content {
  // Content flows naturally within the shell-content constraint
}

// WRONG -- do not duplicate the container
:host {
  max-width: var(--bloom-container-xl);
  margin: 0 auto;
  padding: var(--bloom-space-8); // This padding is already on shell-content
}
```

#### Narrow Content Page (forms, settings)

For pages that should be narrower (e.g., settings forms, profile editing), use the `bloom-container-narrow` utility class (42rem / 672px max-width):

```html
<div class="bloom-container-narrow">
  <h1>Settings</h1>
  <!-- form content -->
</div>
```

#### Full-Screen Page (within Minimal Layout)

Login and register pages should fill the viewport and center their content:

```scss
:host {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100dvh;
  padding: var(--bloom-space-4);
}
```

### 4.3 Grid Patterns

Use utility classes for common grid layouts:

**Responsive auto-fill grids** (for card lists, search results):
```html
<!-- Cards: min 280px each, auto-fill remaining space -->
<div class="bloom-grid-auto-md bloom-gap-6">
  <bloom-card>...</bloom-card>
  <bloom-card>...</bloom-card>
  <bloom-card>...</bloom-card>
</div>
```

| Class | Min column width | Best for |
|-------|-----------------|----------|
| `bloom-grid-auto-sm` | 200px | Badge collections, small items, tag groups |
| `bloom-grid-auto-md` | 280px | Cards, search results, WatchSpace lists |
| `bloom-grid-auto-lg` | 360px | Large cards, detail panels, wide content |

**Fixed column grids**:
```html
<!-- 2-column on all screen sizes -->
<div class="bloom-grid bloom-grid-cols-2 bloom-gap-6">
  <div>Left</div>
  <div>Right</div>
</div>
```

For responsive behavior, combine fixed grids with media-query overrides in component SCSS rather than relying on utility classes.

### 4.4 Flex Patterns

**Horizontal row with centered items** (common for toolbars, action bars):
```html
<div class="bloom-row bloom-gap-3">
  <bloom-button variant="primary" size="sm">Save</bloom-button>
  <bloom-button variant="ghost" size="sm">Cancel</bloom-button>
</div>
```

**Vertical stack** (common for form layouts, content sections):
```html
<div class="bloom-stack bloom-gap-4">
  <bloom-input label="Name" />
  <bloom-input label="Email" />
  <bloom-button type="submit">Submit</bloom-button>
</div>
```

**Space-between row** (common for header + action pairs):
```html
<div class="bloom-flex bloom-justify-between bloom-items-center">
  <h2>Watch Spaces</h2>
  <bloom-button variant="primary" size="sm">Create New</bloom-button>
</div>
```

### 4.5 Responsive Breakpoint Strategy

BloomWatch is mobile-first. Base styles target the smallest viewport. Enhancements are added at larger breakpoints.

**Breakpoints (SCSS variables for use in `@media` queries)**:

| Variable | Value | Name | Target |
|----------|-------|------|--------|
| `$bloom-bp-xs` | 375px | Extra small | Large phones |
| `$bloom-bp-sm` | 640px | Small | Small tablets, landscape phones |
| `$bloom-bp-md` | 768px | Medium | Tablets |
| `$bloom-bp-lg` | 1024px | Large | Small desktops, landscape tablets |
| `$bloom-bp-xl` | 1280px | Extra large | Desktops |
| `$bloom-bp-2xl` | 1536px | 2XL | Large desktops |

**Breakpoint mixins** (preferred over raw `@media` queries):

```scss
@use 'app/shared/styles/tokens' as *;

.my-component {
  // Mobile-first base styles
  padding: var(--bloom-space-4);

  @include bp-md {
    padding: var(--bloom-space-6);
  }

  @include bp-lg {
    padding: var(--bloom-space-8);
  }
}
```

**Available mixins**: `bp-xs`, `bp-sm`, `bp-md`, `bp-lg`, `bp-xl`, `bp-2xl`, `bp-mobile-only`, `bp-tablet-only`.

**Rule**: Write all base styles for mobile first. Only use `min-width` breakpoints to add enhancements. The `bp-mobile-only` and `bp-tablet-only` mixins exist for hiding/showing elements at specific ranges but should be used sparingly.

### 4.6 Navigation

The Shell Layout provides a sticky top navigation bar with:
- Brand link (BloomWatch logotype with gradient text and flower icon)
- Horizontal nav links on desktop
- Hamburger menu + slide-down dropdown on mobile (< 768px)
- Active link styling via `routerLinkActive="shell-nav__link--active"`

Navigation links are rendered as `<a>` elements with `routerLink`. Each link has:
- A decorative icon span (hidden from assistive tech via `aria-hidden="true"`)
- ARIA `role="menuitem"` within a `role="menubar"` list
- Click handler that closes the mobile menu (`closeMobile()`)

When adding new navigation items, follow this pattern:

```html
<li role="none">
  <a
    routerLink="/new-route"
    routerLinkActive="shell-nav__link--active"
    class="shell-nav__link"
    role="menuitem"
    (click)="closeMobile()"
  >
    <span class="shell-nav__link-icon" aria-hidden="true">ICON</span>
    Link Label
  </a>
</li>
```

Keep navigation items to 5-7 maximum. The current set (Dashboard, Watch Spaces, Settings, Showcase) is within budget. Showcase should be removed or hidden behind a dev flag before production.

---

## 5. Interaction & Motion Design

### 5.1 Animation Principles

1. **Purpose over decoration**. Every animation must serve one of: guiding attention, providing feedback, communicating state change, or orienting the user in space. If you cannot name which of these an animation serves, remove it.

2. **Fast feedback, slow reveals**. Interactive feedback (button press, focus ring, hover state) uses `--bloom-duration-fast` (150ms). Content entrance (page load, section reveal) uses `--bloom-duration-slow` (400ms). This creates a responsive feel for interactions while giving content transitions enough time to be perceived.

3. **Spring and bounce are intentional**. The `--bloom-ease-bounce` and `--bloom-ease-spring` curves give BloomWatch its kawaii character. Use them for hover-lift effects and playful micro-interactions. Do not use them for content transitions or page navigation -- those should use `--bloom-ease-out`.

4. **Stagger for hierarchy**. When revealing a list of items, use `bloom-stagger-children` to animate them sequentially with 60ms delay per item. This creates visual flow and reduces cognitive load. Maximum 12 staggered children.

### 5.2 Hover / Focus / Active Conventions

All interactive elements must have all three states defined:

**Hover** (`:hover:not(:disabled)`):
- Buttons: `translateY(-2px)` lift + shadow escalation one level
- Cards (hoverable): `translateY(-4px)` lift + shadow escalation two levels + border color change to `--bloom-pink-200`
- Nav links: Background tint (`--bloom-pink-50`), border reveal (`--bloom-pink-100`), `translateY(-1px)` micro-lift
- Avatar stack items: `translateY(-2px)` with z-index elevation

**Focus** (`:focus-visible`):
- Universal: 2px solid `--bloom-border-focus` (`--bloom-pink-400`) outline, 2px offset
- Inputs: Replace outline with pink ring shadow (`0 0 0 3px rgb(255 45 138 / 0.12)`) plus border color change
- Never use `:focus` for visual styles (it fires on click). Always use `:focus-visible`.

**Active** (`:active:not(:disabled)`):
- Buttons: `translateY(1px)` press-down + shadow de-escalation + gel-shine opacity reduction
- Cards: `translateY(-1px)` reduced lift + shadow de-escalation one level
- Nav links: `translateY(0)` return to baseline

### 5.3 Page Transition Patterns

Content within `shell-content` enters with `bloom-fade-in-up` (opacity 0 -> 1, translateY 8px -> 0) over `--bloom-duration-slow`. This is applied once on the shell-content container, not on individual page components.

Individual pages should NOT add their own entrance animation on the `:host` element, as this would compound with the shell-content animation. If a page needs staggered section reveals, apply animations to internal sections, not the host.

### 5.4 Loading State Patterns

**Button loading**: Use the built-in `[loading]="true"` input. The three-dot bounce animation (`bloom-loading-dot` keyframes) plays within the button's existing dimensions.

**Content loading** (skeleton screens): Use `bloom-animate-shimmer` on placeholder elements. The shimmer moves a white-to-transparent gradient left-to-right on a 2-second loop.

```html
<!-- Skeleton card while loading -->
<bloom-card [hoverable]="false">
  <div class="skeleton-line skeleton-line--title bloom-animate-shimmer"></div>
  <div class="skeleton-line bloom-animate-shimmer"></div>
  <div class="skeleton-line bloom-animate-shimmer"></div>
</bloom-card>
```

**Spinner loading** (for standalone spinners): Use `bloom-animate-spin` for a standard 0.8s rotation, or `bloom-animate-spin-slow` for a gentle 3s rotation.

**Pulse loading** (for indicating live state): Use `bloom-animate-pulse` (opacity pulse) or `bloom-animate-pulse-scale` (scale pulse).

### 5.5 Micro-Interaction Guidelines

**Appropriate micro-interactions** (use these):
- `bloom-hover-lift`: Cards, clickable surfaces. Lift + shadow escalation.
- `bloom-hover-grow`: Thumbnails, icons on hover. Subtle 3% scale increase.
- `bloom-hover-glow-pink`: CTAs that need extra emphasis on hover.
- `bloom-animate-wobble`: Success confirmation on an element (single play, not looping).
- `bloom-animate-jelly`: Button press confirmation (single play).
- `bloom-animate-bounce-in`: Element appearing for the first time (modal opening, toast entering).

**Inappropriate micro-interactions** (avoid these):
- `bloom-animate-bounce` on content that should be static. Continuous bounce is for decorative elements only.
- `bloom-animate-float` on UI controls. Floating elements are hard to click.
- Multiple glow animations on the same screen. Pick one element to glow.

### 5.6 Reduced Motion

Every animation and transition in BloomWatch has a `prefers-reduced-motion: reduce` override. The global override in `_animations.scss` sets all animation durations to `0.01ms` and iteration counts to 1, and removes hover transforms.

When adding new animations, always include a reduced-motion block:

```scss
.my-animation {
  animation: my-keyframes 0.5s ease;
  transition: transform var(--bloom-transition-normal);
}

@media (prefers-reduced-motion: reduce) {
  .my-animation {
    animation: none;
    transition: none;
  }
}
```

The `@include reduced-motion { ... }` mixin from `_tokens.scss` is available for convenience.

---

## 6. Accessibility Standards

### 6.1 Baseline

WCAG 2.1 Level AA is the minimum standard. All UI in BloomWatch must meet this bar. This is non-negotiable.

### 6.2 Focus Management

- **Focus visibility**: `:focus-visible` produces a 2px solid `--bloom-pink-400` outline with 2px offset on all interactive elements. This is set globally in `_base.scss`. Component-level focus styles may enhance but must not remove this indicator.
- **Focus within inputs**: Input components use a shadow ring instead of an outline (because the outline would conflict with the rounded border). This is acceptable as long as the visual change is perceivable at 3:1 contrast.
- **Tab order**: Follow the natural DOM order. Do not use `tabindex` values greater than 0. Use `tabindex="0"` only on elements that are not natively focusable but need to be (custom widgets). Use `tabindex="-1"` to make elements programmatically focusable without adding them to the tab order.
- **Focus trapping**: Modals (when built) must trap focus within their boundaries. Use Angular CDK's `FocusTrap` or equivalent.
- **Skip link**: A `.skip-link` class is defined in `_base.scss`. The main layout should include a skip link as the first focusable element in `body`.

### 6.3 Color Contrast

- **Text on surfaces**: `--bloom-text-primary` (`--bloom-neutral-900`: #211a33) on `--bloom-surface-base` (`--bloom-neutral-0`: #ffffff) provides a contrast ratio exceeding 15:1. This is the standard text/surface pairing.
- **Secondary text**: `--bloom-text-secondary` (`--bloom-neutral-600`: #6b6085) on white provides approximately 5.5:1 contrast. Acceptable for AA normal text.
- **Tertiary text**: `--bloom-text-tertiary` (`--bloom-neutral-500`: #8e82a6) on white provides approximately 3.5:1 contrast. This is ONLY acceptable for large text (18px+) or non-critical helper text. Do not use tertiary text for information the user needs.
- **Disabled text**: `--bloom-text-disabled` (`--bloom-neutral-400`) does not need to meet contrast requirements per WCAG, but ensure the disabled state is conveyed through more than just color (opacity reduction, cursor change).
- **Button text**: All gel-button variants use white text on saturated gradient backgrounds. Verify contrast against the darkest stop of the gradient (the lower portion). The current button designs meet this requirement.
- **Badge text**: Each badge color variant uses a 700-shade text on a 100-shade background with a 200-shade border. These pairings meet AA contrast requirements.

### 6.4 Screen Reader Considerations

- **Decorative elements**: Flower icons, sparkle borders, gel-shine overlays, and nav link icons all use `aria-hidden="true"`. Do not expose decorative visuals to the accessibility tree.
- **Status indicators**: Avatar status dots use `role="status"` and `aria-label` with the status name. Badge dots are `aria-hidden="true"` because the badge label text conveys the status.
- **Required fields**: The visual asterisk (`*`) is `aria-hidden="true"`. The HTML `required` attribute on the input communicates the requirement to screen readers.
- **Loading states**: Buttons set `aria-busy="true"` when loading. Page-level loading should use `aria-live="polite"` regions.
- **Screen-reader-only content**: Use the `.sr-only` class for text that should be announced but not visible. The `.sr-only-focusable` variant becomes visible when focused (for skip links).

### 6.5 Keyboard Navigation

- All interactive elements are keyboard-accessible via native HTML semantics (`<button>`, `<a>`, `<input>`).
- The navigation bar uses `role="menubar"` with `role="menuitem"` children. Navigation via arrow keys within the menu is not yet implemented -- this should be added when the nav becomes more complex.
- The mobile menu toggle sets `aria-expanded` and `aria-controls` correctly.
- Cards are not keyboard-focusable by default. The interactive elements within cards (buttons, links) are the focus targets.

### 6.6 ARIA Usage Conventions

- **Do not use ARIA for things HTML already handles.** A `<button>` does not need `role="button"`. An `<a href="...">` does not need `role="link"`.
- **Use `aria-label`** for elements where the visual content is insufficient (icon-only controls, avatar stacks, badges that need additional context).
- **Use `aria-describedby`** to link inputs to their hints and error messages (already implemented in `bloom-input`).
- **Use `aria-invalid="true"`** on inputs with validation errors (already implemented in `bloom-input`).
- **Use `aria-disabled`** in addition to the HTML `disabled` attribute on custom components that wrap native elements.

---

## 7. CSS Architecture Rules

### 7.1 SCSS Organization

**Global styles** live in `src/app/shared/styles/` and are loaded via `src/styles.scss`:

| File | Purpose | Load order |
|------|---------|------------|
| `_tokens.scss` | CSS custom properties, SCSS variables, breakpoint mixins, utility mixins | 1st |
| `_base.scss` | CSS reset, element defaults, scrollbar styles, focus styles, accessibility utilities | 2nd |
| `_animations.scss` | Keyframe definitions, animation utility classes, hover interaction classes, stagger utilities, reduced-motion overrides | 3rd |
| `_utilities.scss` | Layout utilities, spacing utilities, typography utilities, color utilities, decorative utilities, responsive visibility | 4th |

**Component styles** are co-located with their components and scoped via Angular's view encapsulation:
- `src/app/shared/ui/<name>/bloom-<name>.scss` for shared UI components
- `src/app/features/<feature>/<component>.scss` for feature-specific components
- `src/app/core/layout/<layout>/<layout>.scss` for layout components

### 7.2 BEM Naming Convention

All component CSS uses BEM (Block Element Modifier):

```
.bloom-<block>                      -- Block
.bloom-<block>__<element>           -- Element
.bloom-<block>--<modifier>          -- Modifier
.bloom-<block>__<element>--<modifier> -- Element modifier
```

Examples from the existing codebase:
```scss
.bloom-btn                     // Block
.bloom-btn__label              // Element
.bloom-btn__icon               // Element
.bloom-btn__icon--left         // Element modifier
.bloom-btn__spinner-dot        // Element (nested for clarity, not nested BEM)
.bloom-btn--primary            // Block modifier
.bloom-btn--sm                 // Block modifier
.bloom-btn--loading            // State modifier
.bloom-btn--full-width         // State modifier
```

**Rules**:
- Never nest BEM more than one level: `.bloom-btn__spinner-dot` is fine. `.bloom-btn__spinner__dot__inner` is not.
- Modifiers on the block, not elements, when the modifier changes the entire block: `.bloom-btn--primary` (affects the whole button) vs `.bloom-btn__icon--left` (affects only the icon position).
- State modifiers use the same BEM pattern: `--loading`, `--disabled`, `--focused`, `--error`, `--active`.

### 7.3 When to Use Utilities vs. Component Styles

**Use utility classes** for:
- Quick layout within page templates (flex, grid, gap, padding, margin)
- One-off spacing or alignment adjustments
- Text color/size changes in content areas
- Decorative additions (gradient text, glass effect, glow borders)

```html
<!-- Utility classes for page layout -->
<div class="bloom-flex bloom-justify-between bloom-items-center bloom-mb-6">
  <h2>My Watch Spaces</h2>
  <bloom-button variant="primary" size="sm">Create</bloom-button>
</div>
```

**Use component SCSS** for:
- All shared UI component internal styling
- Complex, reusable visual patterns
- Anything that needs pseudo-elements (::before, ::after)
- Hover/focus/active state chains
- Responsive behavior specific to a component

**Never** use utility classes inside shared UI component templates. Shared components must be self-contained with their own SCSS. Utility classes are for consumption in feature templates and page layouts.

### 7.4 Specificity Management

- Global styles in `_base.scss` use low specificity (element selectors, single-class selectors).
- Component styles use single-class BEM selectors. No nesting beyond what BEM requires.
- Never use `!important` in component styles. The only `!important` declarations in the system are in responsive visibility utilities (`bloom-hide-mobile`, etc.), where they are necessary to override unknown display values.
- Never use ID selectors in CSS.
- Never use inline styles in templates (the showcase page uses inline styles for demonstration only -- do not replicate this in production code).

### 7.5 Importing Tokens in Component SCSS

To use SCSS mixins and variables (breakpoints, `reduced-motion`, `bloom-focus-ring`, `gel-shine`, `truncate`) in a component's SCSS file:

```scss
@use 'app/shared/styles/tokens' as *;

.my-component {
  padding: var(--bloom-space-4);

  @include bp-md {
    padding: var(--bloom-space-6);
  }

  &:focus-visible {
    @include bloom-focus-ring;
  }

  @include reduced-motion {
    transition: none;
  }
}
```

CSS custom properties (`var(--bloom-*)`) do not require any import -- they are globally available.

### 7.6 Component Style Scoping

Angular's default `ViewEncapsulation.Emulated` is used (the default for standalone components). This means:
- Each component's styles are scoped with attribute selectors.
- Styles do not leak out of the component.
- `:host` is used to style the component's host element.
- `::ng-deep` is **banned**. If you need to style a child component, use CSS custom properties or content projection.

---

## 8. Angular Component Patterns

### 8.1 Standalone Components

Every component in BloomWatch is standalone. The `standalone: true` flag is required. There are no NgModules for component declaration.

```typescript
@Component({
  selector: 'bloom-example',
  standalone: true,
  imports: [NgClass],  // Import only what's used
  styleUrl: './bloom-example.scss',
  template: `...`,
})
export class BloomExampleComponent { }
```

### 8.2 Signal-Based Inputs and Outputs

BloomWatch uses Angular's signal-based API. The decorator-based API (`@Input()`, `@Output()`) is prohibited.

```typescript
// CORRECT
readonly variant = input<BloomButtonVariant>('primary');
readonly disabled = input<boolean>(false);
readonly clicked = output<MouseEvent>();

// WRONG -- do not use decorators
@Input() variant: BloomButtonVariant = 'primary';
@Output() clicked = new EventEmitter<MouseEvent>();
```

**Rules**:
- All inputs are `readonly` and declared with `input<Type>(defaultValue)`.
- All outputs are `readonly` and declared with `output<Type>()`.
- Derived state uses `computed()`, not getters.
- Mutable internal state uses `signal()`.

```typescript
// Derived state
readonly buttonClasses = computed(() => ({
  'bloom-btn': true,
  [`bloom-btn--${this.variant()}`]: true,
}));

// Internal mutable state
readonly isFocused = signal(false);
```

### 8.3 Content Projection

BloomWatch uses named `ng-content` slots with attribute selectors for multi-slot projection:

```html
<!-- Component template -->
<header class="bloom-card__header">
  <ng-content select="[bloomCardHeader]" />
</header>
<div class="bloom-card__body">
  <ng-content />  <!-- default slot -->
</div>
<footer class="bloom-card__footer">
  <ng-content select="[bloomCardFooter]" />
</footer>
```

```html
<!-- Consumer template -->
<bloom-card>
  <h3 bloomCardHeader>Title</h3>
  <p>Body content goes in the default slot.</p>
  <div bloomCardFooter>Footer content</div>
</bloom-card>
```

**Naming convention**: Slot selectors use camelCase attribute names prefixed with the component name: `bloomCardHeader`, `bloomCardFooter`, `bloomButtonIconLeft`, `bloomInputPrefix`, etc.

**Empty slot handling**: Always add `&:empty { display: none; }` to slot wrapper elements so that unused slots don't create empty space.

### 8.4 ControlValueAccessor for Form Components

All form-field components must implement `ControlValueAccessor` to integrate with Angular's forms API. The `bloom-input` component demonstrates the canonical pattern:

```typescript
@Component({
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => BloomInputComponent),
      multi: true,
    },
  ],
})
export class BloomInputComponent implements ControlValueAccessor {
  readonly value = signal('');

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(val: string): void {
    this.value.set(val ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  onInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.value.set(target.value);
    this.onChange(target.value);
    this.valueChange.emit(target.value);
  }

  onBlur(): void {
    this.isFocused.set(false);
    this.onTouched();
  }
}
```

All new form components (textarea, select, toggle, checkbox) must follow this pattern.

### 8.5 Component File Structure

**Shared UI components** (reusable across features):
```
src/app/shared/ui/<name>/
  bloom-<name>.ts             # Component + types
  bloom-<name>.scss           # Styles
```

Inline templates are used when the template is reasonably short (as seen in all current components). If a template exceeds approximately 50 lines, extract it to a `.html` file and use `templateUrl`.

**Feature components** (specific to one feature):
```
src/app/features/<feature>/
  <component>.ts              # Component
  <component>.html            # Template (if external)
  <component>.scss            # Styles (if needed)
  <feature>.routes.ts         # Feature routes
```

### 8.6 Barrel Export Maintenance

All shared UI components must be exported from `src/app/shared/ui/index.ts`. When adding a new component:

1. Create the component files.
2. Add the export to `index.ts`:
```typescript
// NewComponent
export { BloomNewComponent } from './new-component/bloom-new-component';
export type { BloomNewComponentVariant } from './new-component/bloom-new-component';
```
3. Feature components import from the barrel: `import { BloomNewComponent } from '../../shared/ui';`

### 8.7 Route Configuration

Routes use lazy-loaded standalone components and child route files:

```typescript
// Feature routes file (e.g., watch-spaces.routes.ts)
import { Routes } from '@angular/router';

export const watchSpacesRoutes: Routes = [
  { path: '', loadComponent: () => import('./watch-space-list').then(m => m.WatchSpaceList) },
  { path: ':id', loadComponent: () => import('./watch-space-detail').then(m => m.WatchSpaceDetail) },
];
```

All feature routes are lazy-loaded via `loadChildren` in the top-level `app.routes.ts`. Individual components within a feature are lazy-loaded via `loadComponent`.

---

## 9. Common UI Patterns

### 9.1 Form Layouts

#### Authentication Forms (Login / Register)

Rendered within `MinimalLayout`. Centered in the viewport. Narrow width (max 24rem-28rem).

```html
<div class="auth-page">
  <bloom-card>
    <div bloomCardHeader class="auth-header">
      <h1>Log In</h1>
      <p>Welcome back to BloomWatch</p>
    </div>

    <form class="bloom-stack bloom-gap-4">
      <bloom-input
        label="Email"
        type="email"
        placeholder="you@example.com"
        [required]="true"
        autocomplete="email"
      />
      <bloom-input
        label="Password"
        type="password"
        placeholder="Your password"
        [required]="true"
        autocomplete="current-password"
      />
      <bloom-button type="submit" [fullWidth]="true" [loading]="isSubmitting()">
        Log In
      </bloom-button>
    </form>

    <div bloomCardFooter class="bloom-text-center">
      <p class="bloom-text-sm bloom-text-secondary">
        Don't have an account? <a routerLink="/register">Sign up</a>
      </p>
    </div>
  </bloom-card>
</div>
```

**Key rules for auth forms**:
- Always set `autocomplete` attributes correctly (`email`, `current-password`, `new-password`, `username`).
- Use `fullWidth` on the submit button.
- Show loading state on the button during submission.
- Use `type="submit"` on the submit button and handle the form's `(ngSubmit)` event.

#### Settings Forms

Rendered within `ShellLayout`. Use `bloom-container-narrow` for constraint. Stack inputs vertically with `bloom-gap-4`.

#### Search / Filter Bars

Use `sm` or `md` sized inputs inline with other controls:

```html
<div class="bloom-row bloom-gap-3 bloom-flex-wrap">
  <bloom-input placeholder="Search anime..." size="sm" class="bloom-flex-1">
    <span bloomInputPrefix>SEARCH_ICON</span>
  </bloom-input>
  <bloom-badge color="pink">Romance</bloom-badge>
  <bloom-badge color="blue">Action</bloom-badge>
</div>
```

### 9.2 List / Detail Navigation

**List page** (e.g., WatchSpace list):
- Page title + "Create" button in a space-between header row
- `bloom-grid-auto-md` grid for card items
- Each card navigates to the detail page via a button or routerLink inside the card footer

**Detail page** (e.g., WatchSpace detail):
- Back link or breadcrumb at the top
- Title + metadata header section
- Content sections below, separated by `--bloom-space-8` vertical rhythm

### 9.3 Empty States

When a list or section has no content, show a centered message within a card:

```html
<bloom-card [hoverable]="false">
  <div class="bloom-center bloom-stack bloom-gap-4 bloom-py-8 bloom-text-center">
    <span class="bloom-text-3xl" aria-hidden="true">DECORATIVE_ICON</span>
    <h3>No Watch Spaces Yet</h3>
    <p class="bloom-text-secondary">Create your first Watch Space to start tracking anime with friends.</p>
    <bloom-button variant="primary">Create Watch Space</bloom-button>
  </div>
</bloom-card>
```

**Rules for empty states**:
- Always include a clear heading explaining what is empty.
- Always include a description of what the user should do.
- Always include a CTA button to resolve the empty state.
- Use a decorative icon (aria-hidden) for visual warmth but never let it be the sole communicator.

### 9.4 Error States

#### Field-Level Errors

Handled by `bloom-input`'s `error` input. The field shows red border, red background tint, shake animation on the error message, and `aria-invalid` + `role="alert"`.

#### Page-Level Errors

For API errors, failed loads, or unrecoverable states:

```html
<bloom-card [hoverable]="false">
  <div class="bloom-center bloom-stack bloom-gap-4 bloom-py-8 bloom-text-center">
    <span class="bloom-text-3xl" aria-hidden="true">ERROR_ICON</span>
    <h3 class="bloom-text-error">Something Went Wrong</h3>
    <p class="bloom-text-secondary">We couldn't load your Watch Spaces. Please try again.</p>
    <bloom-button variant="secondary" (clicked)="retry()">Try Again</bloom-button>
  </div>
</bloom-card>
```

**Rules for error states**:
- Use `--bloom-error-dark` for error heading text.
- Provide a retry action when possible.
- Do not blame the user. Use neutral, friendly language.
- Log the technical error to the console/monitoring; show a human-readable message to the user.

#### Inline Error Banners

For non-blocking errors (e.g., sync failure while the page still shows stale data):

```html
<div class="bloom-flex bloom-gap-3 bloom-p-4 bloom-rounded-lg"
     style="background: var(--bloom-error-bg); border: 1px solid var(--bloom-error-light);"
     role="alert">
  <span class="bloom-text-error">ERROR_ICON</span>
  <p class="bloom-text-sm">AniList sync failed. Your data may be outdated.
    <a href="#" (click)="retrySync(); $event.preventDefault()">Retry</a>
  </p>
</div>
```

### 9.5 Loading States

**Full page loading** (first load of a feature):
Use skeleton cards in the same grid layout as the eventual content. This prevents layout shift.

**Section loading** (loading a sub-section):
Use `bloom-animate-pulse` on a placeholder, or `bloom-animate-shimmer` on skeleton lines.

**Action loading** (submitting a form, triggering a sync):
Use `[loading]="true"` on the triggering button. Do not show a full-page spinner for button actions.

### 9.6 Toast / Notification Patterns (Future)

When built, toasts should:
- Use `z-index: var(--bloom-z-toast)` (600).
- Enter with `bloom-bounce-in` animation.
- Auto-dismiss after 5 seconds with a progress bar.
- Use status colors: `--bloom-success-bg` for success, `--bloom-error-bg` for error, `--bloom-info-bg` for info.
- Include a dismiss button.
- Be announced via `aria-live="polite"`.
- Stack vertically from the top-right corner on desktop, bottom-center on mobile.

### 9.7 Modal / Dialog Patterns (Future)

When built, modals should:
- Use `z-index: var(--bloom-z-modal)` (400) with a `--bloom-z-overlay` (300) backdrop.
- The backdrop should use `bloom-glass` (frosted) styling.
- The modal card should use `bloom-card` with `bloom-animate-bounce-in` entrance.
- Focus must be trapped within the modal.
- Escape key must close the modal.
- `aria-modal="true"` and `role="dialog"` must be set.
- The modal should be rendered at the body level (or via Angular CDK overlay) to avoid stacking context issues.

### 9.8 Confirmation Patterns (Future)

Destructive actions must always require confirmation. The confirmation dialog should:
- Clearly state what will be destroyed.
- Name the specific item (e.g., "Delete Watch Space 'Anime Night'?").
- Use a `danger` button for the destructive action.
- Use a `ghost` button for cancellation.
- Default focus should be on the cancel button, not the destructive button.

---

## 10. Anti-Patterns (What NOT to Do)

### 10.1 Token Violations

**NEVER hardcode color values.**
```scss
// WRONG
color: #ff2d8a;
background: #fff0f6;
border: 1px solid #e8e3ef;

// CORRECT
color: var(--bloom-primary);
background: var(--bloom-surface-tinted);
border: 1px solid var(--bloom-border-default);
```

**NEVER hardcode spacing values.**
```scss
// WRONG
padding: 16px;
margin-bottom: 24px;
gap: 8px;

// CORRECT
padding: var(--bloom-space-4);
margin-bottom: var(--bloom-space-6);
gap: var(--bloom-space-2);
```

**NEVER hardcode font sizes, weights, or families.**
```scss
// WRONG
font-size: 14px;
font-weight: 600;
font-family: 'Quicksand', sans-serif;

// CORRECT
font-size: var(--bloom-text-sm);
font-weight: var(--bloom-font-semibold);
font-family: var(--bloom-font-family-display);
```

**NEVER hardcode border-radius values.**
```scss
// WRONG
border-radius: 16px;

// CORRECT
border-radius: var(--bloom-radius-lg);
```

**NEVER hardcode z-index values.**
```scss
// WRONG
z-index: 9999;

// CORRECT
z-index: var(--bloom-z-modal);
```

### 10.2 Component Misuse

**NEVER use `::ng-deep`.** It breaks encapsulation and creates unmaintainable specificity chains. If a child component needs customization, use CSS custom properties or content projection.

**NEVER wrap a `<bloom-card>` in an `<a>` tag.** Cards are not links. Put interactive elements inside the card's projected content.

**NEVER put a `primary` button next to another `primary` button.** Use `primary` + `ghost` or `primary` + `secondary`.

**NEVER use a `danger` button for non-destructive actions.** Red means "this will remove or destroy something."

**NEVER use `bloom-animate-float` or `bloom-animate-bounce` on interactive controls.** Moving elements are harder to click and violate WCAG 2.1 Success Criterion 2.3.3.

**NEVER place inline styles in production templates.** The showcase page uses inline styles for demonstration brevity. Production code must use component SCSS or utility classes.

### 10.3 Layout Mistakes

**NEVER add max-width or container padding to pages rendered within ShellLayout.** The `shell-content` wrapper already provides these constraints.

**NEVER use `position: fixed` for elements that should be `sticky`.** The nav bar is sticky, not fixed. Fixed positioning removes elements from the document flow and causes content overlap issues.

**NEVER create new breakpoint values.** Use the defined `$bloom-bp-*` variables. If a component doesn't need all breakpoints, skip the irrelevant ones rather than inventing intermediate values.

### 10.4 Accessibility Violations

**NEVER remove focus outlines without providing an alternative.** The global `:focus { outline: none; }` in base styles is paired with `:focus-visible` styles. If you remove these, keyboard users cannot navigate.

**NEVER use color alone to convey information.** The badge dot, for example, has both color AND animation to indicate status. Error states use both red color AND shake animation AND error text.

**NEVER use `aria-hidden="true"` on elements that contain interactive children.** This removes both the element and its children from the accessibility tree.

**NEVER auto-play audio or video.** This violates WCAG 1.4.2.

**NEVER use `tabindex` values greater than 0.** This creates unpredictable tab order.

### 10.5 Performance Mistakes

**NEVER animate `width`, `height`, `top`, `left`, or `margin`.** These trigger layout recalculation. Use `transform` and `opacity` only for animations. All existing BloomWatch animations follow this rule.

**NEVER use `backdrop-filter` on large or frequently-repainting elements.** It is acceptable on the nav bar (small, static) and small overlays. Do not apply it to scrolling content areas.

**NEVER add `box-shadow` transitions to elements within a scrolling list.** Shadow transitions during scroll cause jank. Apply shadow transitions only on hover, which pauses scrolling implicitly.

**NEVER forget the `anyComponentStyle` budget.** Angular's build config enforces an 8kB warning / 12kB error limit per component stylesheet. Keep component styles focused and lean. Extract shared patterns to global utilities rather than duplicating them.

### 10.6 Things That Look Kawaii but Hurt Usability

**Rainbow text on body copy.** Gradient text (`bloom-gradient-text`) is for headings and decorative display text only. Running gradient text over a paragraph destroys readability.

**Sparkle borders on everything.** The animated sparkle border (`bloom-card--highlighted`, `bloom-sparkle-border`) should appear on 1-2 elements per page at most. When everything sparkles, nothing stands out.

**Excessive bounce animations.** One or two bouncing elements per page is charming. Five is a carnival. Zero is appropriate for data-heavy views.

**Tiny text in kawaii fonts.** Quicksand at very small sizes (below 12px) becomes hard to read due to its rounded letterforms. Never use the display font below `--bloom-text-xs` (12px). For very small text, use the body font (Nunito) instead.

**Pastel-on-pastel text.** Pink text on a pink-tinted background, lilac text on a lilac card -- these combinations fail contrast requirements and are hard to read even for sighted users. Always check the contrast between your text color and its immediate background.

---

## Appendix: Quick Reference Card

### File Paths

| What | Where |
|------|-------|
| Design tokens | `src/app/shared/styles/_tokens.scss` |
| Base reset styles | `src/app/shared/styles/_base.scss` |
| Animation system | `src/app/shared/styles/_animations.scss` |
| Utility classes | `src/app/shared/styles/_utilities.scss` |
| Global entry point | `src/styles.scss` |
| Shared UI components | `src/app/shared/ui/` |
| UI barrel export | `src/app/shared/ui/index.ts` |
| Shell layout | `src/app/core/layout/shell-layout/` |
| Minimal layout | `src/app/core/layout/minimal-layout/` |
| Feature modules | `src/app/features/<feature>/` |
| Application routes | `src/app/app.routes.ts` |

### Token Prefix Cheat Sheet

All tokens begin with `--bloom-`. The second segment indicates the category:

| Prefix | Category | Example |
|--------|----------|---------|
| `--bloom-pink-*` | Pink palette | `--bloom-pink-500` |
| `--bloom-blue-*` | Blue palette | `--bloom-blue-300` |
| `--bloom-lilac-*` | Lilac palette | `--bloom-lilac-400` |
| `--bloom-lime-*` | Lime palette | `--bloom-lime-200` |
| `--bloom-peach-*` | Peach palette | `--bloom-peach-500` |
| `--bloom-yellow-*` | Yellow palette | `--bloom-yellow-100` |
| `--bloom-neutral-*` | Neutral palette | `--bloom-neutral-600` |
| `--bloom-surface-*` | Surface colors | `--bloom-surface-raised` |
| `--bloom-text-*` (color) | Text colors | `--bloom-text-secondary` |
| `--bloom-text-*` (size) | Font sizes | `--bloom-text-lg` |
| `--bloom-font-*` | Font properties | `--bloom-font-family-display` |
| `--bloom-leading-*` | Line heights | `--bloom-leading-normal` |
| `--bloom-tracking-*` | Letter spacing | `--bloom-tracking-wide` |
| `--bloom-space-*` | Spacing | `--bloom-space-4` |
| `--bloom-radius-*` | Border radius | `--bloom-radius-xl` |
| `--bloom-shadow-*` | Box shadows | `--bloom-shadow-md` |
| `--bloom-duration-*` | Animation duration | `--bloom-duration-fast` |
| `--bloom-ease-*` | Easing functions | `--bloom-ease-bounce` |
| `--bloom-transition-*` | Combined transitions | `--bloom-transition-normal` |
| `--bloom-z-*` | Z-index | `--bloom-z-modal` |
| `--bloom-gradient-*` | Gradients | `--bloom-gradient-kawaii` |
| `--bloom-border-*` | Border colors | `--bloom-border-focus` |
| `--bloom-container-*` | Container widths | `--bloom-container-lg` |
| `--bloom-bp-*` | Breakpoints (ref only) | `--bloom-bp-md` |
| `--bloom-nav-*` | Navigation dimensions | `--bloom-nav-height` |
| `--bloom-sidebar-*` | Sidebar dimensions | `--bloom-sidebar-width` |

### Component Import Pattern

```typescript
import {
  BloomButtonComponent,
  BloomCardComponent,
  BloomInputComponent,
  BloomBadgeComponent,
  BloomAvatarComponent,
  BloomAvatarStackComponent,
} from '../../shared/ui';  // Adjust path depth as needed
```

---

*This document is the law of the land for BloomWatch UI development. When it conflicts with a developer's personal preference, this document wins. When it conflicts with a third-party tutorial, this document wins. When it is silent on a topic, the existing codebase patterns are the next authority, followed by WCAG 2.1 AA guidelines, followed by Angular best practices.*
