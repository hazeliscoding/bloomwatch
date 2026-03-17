## Why

BloomWatch needs a public-facing landing page as the entry point for new visitors. Currently the root route (`/`) falls through to the authenticated shell layout, leaving unauthenticated users with no introduction to the product. A dedicated landing page explains what BloomWatch does and funnels visitors toward registration or sign-in, which is a prerequisite for the rest of the auth frontend (Stories 7.2–7.4).

## What Changes

- Add a new `LandingPage` component under `features/landing/` that renders a hero section, feature highlights, and CTA buttons
- Register the component on the `/` route inside the `MinimalLayout` (public, no nav bar)
- Implement responsive layout matching the wireframe (`docs/wireframes/landing.html`)
- Style the page using the kawaii/Y2K design system tokens (gradient text, gel buttons, soft cards, sparkle dividers)
- "Create Account" CTA navigates to `/register`; "Sign In" CTA navigates to `/login`

## Capabilities

### New Capabilities
- `landing-page`: Public landing page component with hero section, feature highlights grid, and auth CTAs — serves as the unauthenticated root route

### Modified Capabilities
- `angular-routing-shell`: The root path (`/`) inside the MinimalLayout must now load the landing page component instead of falling through to the shell layout

## Impact

- **Routes:** The `/` path under `MinimalLayout` gains a default child route for the landing page
- **New files:** `features/landing/landing.ts` component + SCSS
- **Dependencies:** Uses existing shared UI components (`BloomButton`, `BloomCard`) and theme tokens
- **No backend changes** — this is a purely static, frontend-only page
