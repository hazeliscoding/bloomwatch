# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Backend (.NET 10)

```bash
# Run the API (http://localhost:5192, API docs at /scalar/v1)
dotnet run --project src/BloomWatch.Api

# Run all backend tests
dotnet test

# Run tests for a single module
dotnet test tests/BloomWatch.Modules.Identity.IntegrationTests
dotnet test tests/BloomWatch.Modules.Identity.UnitTests

# Apply all EF Core migrations
./scripts/apply-migrations.sh

# Add a migration to a specific module (example: Identity)
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Identity/BloomWatch.Modules.Identity.Infrastructure \
  --startup-project src/BloomWatch.Api \
  --context IdentityDbContext
```

### Frontend (Angular 21)

```bash
cd src/BloomWatch.UI

npm start          # Dev server at http://localhost:4200
npm test           # Vitest tests
npm run build      # Production build
```

## Architecture

BloomWatch is a **modular monolith**: five domain modules sharing one deployment but separated by database schema and bounded context. Modules communicate only through read-only cross-module `DbContext`s — never by calling each other's services or repositories directly.

### Backend Module Structure

Each module follows this DDD layering:

```
src/Modules/<Name>/
├── BloomWatch.Modules.<Name>.Domain/         # Entities, value objects, repository interfaces
├── BloomWatch.Modules.<Name>.Application/    # Use-case handlers (Command/Query), abstractions
├── BloomWatch.Modules.<Name>.Infrastructure/ # EF Core DbContext, repositories, external clients
│   ├── Persistence/                          # DbContext, configurations, migrations
│   ├── CrossModule/                          # Read-only DbContexts for cross-module reads
│   └── Extensions/ServiceCollectionExtensions.cs  # Module registration
└── BloomWatch.Modules.<Name>.Contracts/      # DTOs/types shared outward (optional)
```

**API endpoints** live in `src/BloomWatch.Api/Modules/<Name>/` as static extension classes (`Map*Endpoints()`). Handlers are injected directly — there is no MediatR or dispatcher. The `Home` overview is a thin cross-module orchestrator living directly in `BloomWatch.Api`.

### Modules

| Module | Responsibility |
|---|---|
| **Identity** | Registration, login, JWT auth, user profiles |
| **WatchSpaces** | Shared spaces, invitations, membership |
| **AniListSync** | AniList GraphQL proxy with persistent `MediaCacheEntry` table |
| **AnimeTracking** | Per-space anime lifecycle, participant progress, ratings |
| **Analytics** | Compatibility scores, rating gaps, shared stats (reads other modules via `CrossModule/`) |

### Cross-Module Communication Pattern

When a module needs data owned by another module (e.g., Analytics reading AnimeTracking ratings), it creates a **read-only `DbContext`** in its own `Infrastructure/CrossModule/` folder that targets only the tables it needs. No service calls across modules.

### Database

- PostgreSQL in production; `appsettings.json` default: `Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres`
- Each module owns its own EF Core `DbContext` and schema; migrations are per-module
- Integration tests use in-memory SQLite via `WebApplicationFactory<Program>` — no running database needed

### Frontend Architecture (Angular 21)

```
src/BloomWatch.UI/src/app/
├── core/
│   ├── auth/           # AuthService (signals-based), authGuard, guestGuard
│   ├── http/           # ApiService (thin HttpClient wrapper), authInterceptor
│   ├── layout/         # ShellLayout (nav bar) and MinimalLayout (landing/auth)
│   └── theme/          # ThemeService (light/dark, signal-based, localStorage)
├── features/           # Lazy-loaded route modules: auth, home, watch-spaces, settings
│   └── showcase/       # Dev-only component playground (excluded from prod build)
└── shared/
    ├── styles/          # _tokens.scss, _base.scss, _animations.scss, _utilities.scss
    └── ui/              # bloom-* component library (button, card, input, badge, avatar, modal, compat-ring, backlog-picker, theme-toggle)
```

**Key patterns:**
- Standalone components throughout; no NgModules
- Angular signals for local state (`signal()`, `computed()`); `AuthService` and `ThemeService` are signal-based
- `ApiService` (`core/http/api.service.ts`) is the single HTTP abstraction — all feature services call it
- `authInterceptor` automatically attaches `Authorization: Bearer <token>` to every request
- Dev environment points directly to `http://localhost:5192`; prod uses `/api` (proxied)
- `showcase` route is conditionally included only in non-production builds

### Design System

Components use the `bloom-` prefix. Design tokens (colors, spacing, radii, typography) are CSS custom properties defined in `shared/styles/_tokens.scss`. The palette is kawaii/Y2K pastel with gel effects, Quicksand + Nunito typefaces, and supports light/dark themes. Full doctrine: `docs/ui-ux-doctrine.md`. Wireframes: `docs/wireframes/`.

## OpenSpec Workflow

Features are planned before implementation: `openspec/changes/` contains named change folders with design docs, specs, and task checklists. Completed changes are archived in `openspec/changes/archive/`. Use the `/propose`, `/apply-change`, and `/archive-change` skills to manage this workflow.

## Git Workflow and Releases

### Branching

All work branches off `main`. `main` is always deployable; tagged releases live there.

```
feat/#<issue>-<slug>    # new features
fix/#<issue>-<slug>     # bug fixes
chore/<slug>            # deps, tooling, config
docs/<slug>             # documentation only
```

```bash
git checkout -b feat/#3-refresh-tokens
# ... implement, then open a PR
gh pr create --title "feat: refresh token flow" --body "Closes #3"
```

### Semver

`MAJOR.MINOR.PATCH` — project is pre-1.0, so use `0.MINOR.PATCH`:
- `0.x.y` — bug fixes
- `0.x.0` — new backward-compatible features
- `1.0.0` — all P0 + P1 issues resolved, production deployed

```bash
git tag -a v0.2.0 -m "feat: refresh tokens, password reset"
git push origin v0.2.0
# or with auto-generated release notes
gh release create v0.2.0 --generate-notes --title "v0.2.0"
```

### Milestone targets

| Version | Issues |
|---------|--------|
| v0.2.0 | #2 email delivery · #3 refresh tokens · #4 password reset |
| v0.3.0 | #5 CI/CD · #6 invite-before-signup |
| v1.0.0 | All P0 + P1 issues resolved, production deployed |
