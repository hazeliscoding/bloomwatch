## Context

The dashboard page (Story 10.1) renders the compatibility score inline: SVG ring, computed properties for stroke offset and color, and ~40 lines of SCSS — all embedded in the dashboard component. Story 10.2 extracts this into a reusable shared UI component. The upcoming Analytics page (Story 10.3) will also need to display the compatibility score, confirming the need for extraction now.

The existing inline implementation already works correctly. This is a pure refactor + extraction — no new backend calls, no new data models.

## Goals / Non-Goals

**Goals:**
- Extract a self-contained `BloomCompatRing` component into `shared/ui/compat-ring/`
- Accept `DashboardCompatibility | null` as a single input
- Encapsulate all SVG, computed ring math, color logic, and SCSS
- Replace the dashboard's inline compat section with the new component
- Maintain identical visual output and behavior

**Non-Goals:**
- Changing the compatibility data model or API
- Adding animation beyond the existing stroke-dashoffset transition
- Building the Analytics page (Story 10.3) — that will consume this component later

## Decisions

### 1. Component location — shared UI library

**Decision:** Place the component at `shared/ui/compat-ring/bloom-compat-ring.ts` alongside other `bloom-*` components (card, badge, avatar, button, etc.).

**Rationale:** This follows the existing convention. All reusable UI components live in `shared/ui/<name>/`. The `bloom-` prefix keeps the component library consistent.

### 2. Input interface — accept `DashboardCompatibility | null`

**Decision:** The component accepts a single `compatibility` input of type `DashboardCompatibility | null`. The null case triggers the placeholder state.

**Rationale:** Matches the dashboard API response shape directly. The consumer doesn't need to decompose the object — just pass `data.compatibility` straight through. No additional wrapper type needed.

**Alternative considered:** Separate inputs for `score`, `label`, `ratedTogetherCount` — rejected because it creates unnecessary prop drilling and the object is always available as a unit from the API.

### 3. Ring math — self-contained constants

**Decision:** Move `RING_RADIUS`, `RING_CIRCUMFERENCE`, `ringStrokeDashoffset`, and `ringColor` computations into the new component. Use `computed()` signals that derive from the input.

### 4. Dashboard refactor — replace, don't wrap

**Decision:** Remove the inline compatibility SVG, computed properties, and SCSS from the dashboard entirely. Replace with `<bloom-compat-ring [compatibility]="data.compatibility" />`.

## Risks / Trade-offs

**[Test changes in dashboard spec]** → Existing dashboard tests check for `.dashboard__ring-score` and `dashboard__compat-placeholder`. After extraction, these CSS classes move into the compat ring component. Dashboard tests should verify the component is present; detailed ring behavior tests belong in the new component's spec. → *Mitigation:* Update dashboard tests to check for `bloom-compat-ring` element presence, not internal ring classes.
