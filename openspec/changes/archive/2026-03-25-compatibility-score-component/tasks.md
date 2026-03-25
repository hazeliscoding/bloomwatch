## 1. Create BloomCompatRing Component

- [x] 1.1 Create `shared/ui/compat-ring/bloom-compat-ring.ts` with `compatibility` input, ring math computeds (`ringStrokeDashoffset`, `ringColor`), and SVG ring template with score, label, context, and null placeholder
- [x] 1.2 Create `shared/ui/compat-ring/bloom-compat-ring.scss` with BEM styles for the ring, score text, label, context, placeholder, and reduced-motion support
- [x] 1.3 Write unit tests in `shared/ui/compat-ring/bloom-compat-ring.spec.ts`: ring renders with score, green/yellow/pink color by range, label and context text, null input shows placeholder

## 2. Refactor Dashboard to Use Component

- [x] 2.1 Import `BloomCompatRingComponent` in `watch-space-dashboard.ts` and replace inline compatibility section in the template with `<bloom-compat-ring [compatibility]="data.compatibility" />`
- [x] 2.2 Remove `ringStrokeDashoffset`, `ringColor`, and `ringCircumference` properties from `watch-space-dashboard.ts`
- [x] 2.3 Remove compatibility-specific SCSS from `watch-space-dashboard.scss` (`.dashboard__compat-ring`, `.dashboard__ring-*`, `.dashboard__compat-label`, `.dashboard__compat-context`, `.dashboard__compat-placeholder`, `.dashboard__compat-hint`)
- [x] 2.4 Update `watch-space-dashboard.spec.ts` to verify `bloom-compat-ring` element is present and remove tests that check internal ring classes (those now live in the component's own spec)
