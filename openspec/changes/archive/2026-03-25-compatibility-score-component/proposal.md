## Why

The compatibility score is currently rendered inline within the dashboard template, tightly coupled to the dashboard component's signals and computed properties. Story 10.2 requires extracting it into a standalone, reusable component that accepts the compatibility object as an input. This enables reuse on the upcoming Analytics page (Story 10.3) and keeps the dashboard template focused.

## What Changes

- **New standalone component** `BloomCompatRing` (selector: `bloom-compat-ring`) in the shared UI library that:
  - Accepts a `DashboardCompatibility | null` input
  - Renders an SVG circular progress ring with the score (0–100) in the center
  - Colors the ring contextually: green for 80+, yellow for 50–79, pink for below 50
  - Displays the label text (e.g., "Very synced, with a little spice") beneath the ring
  - Shows `ratedTogetherCount` as supporting context (e.g., "Based on 9 shared ratings")
  - When input is null, renders a soft placeholder message ("Rate more anime together to unlock your compatibility score")
- **Refactor the dashboard** to replace inline compatibility SVG/template/styles with `<bloom-compat-ring [compatibility]="data.compatibility" />`
- **Remove compatibility-specific computed properties** (`ringStrokeDashoffset`, `ringColor`, `ringCircumference`) and SCSS from the dashboard component

## Capabilities

### New Capabilities
- `compat-ring-component`: Standalone reusable compatibility score ring component with input binding, SVG rendering, contextual color, label, context text, and null-state placeholder

### Modified Capabilities
- `watch-space-dashboard`: Dashboard compatibility section delegates to the new `bloom-compat-ring` component instead of rendering inline SVG

## Impact

- **New files**: `shared/ui/compat-ring/bloom-compat-ring.ts`, `bloom-compat-ring.scss`, `bloom-compat-ring.spec.ts`
- **Modified files**: `watch-space-dashboard.ts` (remove compat computeds), `watch-space-dashboard.html` (replace inline section with component), `watch-space-dashboard.scss` (remove compat styles), `watch-space-dashboard.spec.ts` (update compat tests to verify component presence)
- **No backend changes**
- **No new dependencies**
