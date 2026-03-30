## Context

The Angular UI includes a **Showcase** page (`/showcase`) used purely for developer-facing component previews. It is registered as a lazy-loaded child route inside the authenticated `ShellLayout` group in `app.routes.ts` and linked from the shell navigation bar in `shell-layout.html`. The environment configuration already exposes a `production` boolean (`environment.production`) that is `false` during local development and `true` for production builds.

## Goals / Non-Goals

**Goals:**

- Remove the showcase route and its navigation link from production builds so end-users never encounter it.
- Keep the showcase fully functional during local development (`ng serve`).
- Minimal code footprint — leverage Angular's existing environment flag rather than introducing new configuration.

**Non-Goals:**

- Removing or deleting the showcase component source code.
- Introducing a feature-flag system or runtime toggle — the compile-time environment flag is sufficient.
- Protecting the showcase behind an admin role — it simply should not exist in the route table outside development.

## Decisions

### 1. Conditional route registration via environment flag

**Approach**: Build the authenticated children array in `app.routes.ts` dynamically. Spread a showcase route entry only when `environment.production` is `false`.

```ts
...(environment.production ? [] : [
  { path: 'showcase', loadChildren: () => import('./features/showcase/showcase.routes').then(m => m.showcaseRoutes) },
]),
```

**Why over alternatives:**
- *Route guard alternative* — A guard would still register the route and include the chunk reference in the build. Conditional inclusion is cleaner and prevents accidental access entirely.
- *Separate route file per environment* — Adds unnecessary duplication for a single route toggle.

### 2. Conditional navigation link via `@if` + `isDevMode()`

**Approach**: Wrap the showcase `<li>` in the shell layout template with `@if (isDev)` where `isDev` is derived from Angular's `isDevMode()` utility.

**Why `isDevMode()` for the template:**
- Consistent with Angular conventions; tree-shaken in production builds.
- The environment import is usable too, but `isDevMode()` is the idiomatic Angular approach for templates.

### 3. No changes to environment files

The existing `environment.ts` / `environment.development.ts` files already carry `production: true/false`. No modifications needed.

## Risks / Trade-offs

- **[Low] Route still importable in dev tools** → Users in dev mode can navigate to `/showcase` manually. This is acceptable since development mode is local-only.
- **[Low] Forgetting to re-add if showcase is later promoted** → The conditional spread is clearly commented; reducing this risk.
