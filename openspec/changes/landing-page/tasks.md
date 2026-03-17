## 1. Component Scaffolding

- [x] 1.1 Create `features/landing/landing.ts` standalone component with empty template
- [x] 1.2 Create `features/landing/landing.scss` with token-based style scaffolding

## 2. Routing

- [x] 2.1 Add default child route `{ path: '', loadComponent: ... }` for the landing page under the `MinimalLayout` block in `app.routes.ts`

## 3. Hero Section

- [x] 3.1 Add hero section markup: `<h1>` with gradient-text class, subtitle paragraph, and CTA button container
- [x] 3.2 Add "Create Account" `BloomButtonComponent` (primary, lg) wrapped in `<a routerLink="/register">`
- [x] 3.3 Add "Sign In" `BloomButtonComponent` (ghost, lg) wrapped in `<a routerLink="/login">`
- [x] 3.4 Style the hero section: centered layout, title sizing, subtitle color with tokens, CTA gap/flex

## 4. Feature Highlights

- [x] 4.1 Add feature highlights grid section with three `BloomCardComponent` instances (Shared Backlog, Rate & Compare, Discover Together)
- [x] 4.2 Add icon, title, and description content inside each card
- [x] 4.3 Style the feature grid: `auto-fill` grid layout, card padding, icon/title/description typography with tokens

## 5. Responsive & Polish

- [x] 5.1 Add responsive breakpoint styles: single-column grid and reduced hero title size at 640px and below
- [x] 5.2 Add kawaii divider between hero and features sections
- [x] 5.3 Add footer tagline text with tertiary color token

## 6. Verification

- [x] 6.1 Verify `ng build` completes with zero errors
- [x] 6.2 Verify the landing page renders at `/` inside `MinimalLayout` with no nav bar
- [x] 6.3 Verify CTA buttons navigate to `/register` and `/login`
- [x] 6.4 Verify responsive layout at mobile (<=640px) and desktop viewports
