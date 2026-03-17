## Context

BloomWatch's Angular frontend has a working shell with two layout tiers: `ShellLayout` (authenticated, with nav bar) and `MinimalLayout` (public, no nav). The root path `/` currently resolves to the dashboard inside `ShellLayout`. There is no public-facing entry point for unauthenticated visitors.

The wireframe at `docs/wireframes/landing.html` defines the visual structure: a hero section (gradient title, subtitle, two CTAs), a kawaii divider, a 3-column feature highlights grid, and a footer tagline. The UI/UX Doctrine mandates kawaii/Y2K styling with design tokens, soft shadows, gel buttons, and the Quicksand/Nunito font pairing.

Existing shared UI components (`BloomButtonComponent`, `BloomCardComponent`) and SCSS token architecture are in place.

## Goals / Non-Goals

**Goals:**
- Deliver a fully styled, responsive landing page at `/` for unauthenticated visitors
- Use existing shared components and design tokens — no new UI primitives
- Route visitors to `/register` and `/login` via CTAs
- Follow the wireframe layout faithfully

**Non-Goals:**
- Authentication state detection (no redirect logic — that's Story 7.4)
- SEO/meta tags or server-side rendering
- Animation/motion design beyond what the design tokens already provide (gel-shine, hover transitions)
- A/B testing or analytics tracking

## Decisions

### 1. Component location: `features/landing/landing.ts`

Place the landing page under `features/landing/` as a standalone component. This follows the existing feature-based directory convention and keeps it co-located with its SCSS file.

**Alternative considered:** Placing it in `core/` since it's a single page — rejected because `core/` is reserved for singleton services and layout components per the routing shell spec.

### 2. Routing: Add as default child under `MinimalLayout`

Add `{ path: '', loadComponent: () => import('./features/landing/landing').then(m => m.Landing) }` as a child route of the `MinimalLayout` block in `app.routes.ts`. This means `/` resolves to the landing page without the nav bar.

The current `ShellLayout` block also has a `path: ''` child that loads the dashboard. Angular evaluates routes in order, so the `MinimalLayout` block (listed first) will match `/` before the `ShellLayout` block. Authenticated users wanting the dashboard will reach it via `/dashboard` or the nav bar links. Once auth guards exist (Story 7.4), the landing page can redirect authenticated users.

**Alternative considered:** Using a separate `/welcome` path — rejected because the root path is the natural entry point and the acceptance criteria explicitly require `/`.

### 3. Use `BloomButtonComponent` with `routerLink` wrapping

The wireframe shows two CTA buttons: "Create Account" (primary, large) and "Sign In" (ghost, large). Wrap each `<bloom-button>` in an `<a [routerLink]>` anchor to handle navigation. This is simpler than adding router navigation logic inside the button component.

### 4. Feature cards: Use `BloomCardComponent` with custom content projection

The three feature highlight cards (Shared Backlog, Rate & Compare, Discover Together) use `<bloom-card>` with icon, title, and description projected in. No new component needed — the card's content projection handles this.

### 5. SCSS file: `landing.scss` with scoped styles

Component-specific styles (hero sizing, feature grid layout, responsive breakpoints) live in `landing.scss`. All color, spacing, radius, and typography values reference design tokens from the global SCSS token layer — no hardcoded values.

## Risks / Trade-offs

- **Route shadowing:** Adding `/` to `MinimalLayout` means the dashboard is no longer the default root. Once auth guards land (Story 7.4), authenticated users should be redirected from `/` to `/dashboard`. Until then, authenticated users will see the landing page at `/` but can navigate via the nav bar. **Mitigation:** This is acceptable for the current sprint; Story 7.4 will add the redirect guard.

- **No auth-aware rendering:** The landing page always shows "Create Account" / "Sign In" even if the user is already logged in. **Mitigation:** Out of scope per non-goals. Story 7.4 handles redirect for authenticated users.
