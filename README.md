# BloomWatch

A shared anime tracking platform for pairs and small groups. BloomWatch lets friends maintain a joint backlog, log watch history, leave separate ratings, and surface compatibility analytics -- without relying on spreadsheets or Discord threads.

## Tech Stack

**Backend**

- .NET 10 / ASP.NET Core (Minimal APIs)
- PostgreSQL -- one schema per module
- Entity Framework Core 9 + Npgsql
- JWT HS256 (1-hour expiry) + BCrypt password hashing (work factor 12)
- OpenAPI + [Scalar](https://scalar.com/) interactive API docs
- DDD-inspired modular monolith architecture

**Frontend**

- Angular 21 with standalone components and signal-based APIs
- SCSS design system with CSS custom property tokens
- Kawaii/Y2K visual theme (Quicksand + Nunito typefaces, gel effects, pastel palette)
- Vitest for unit testing

**Testing**

- xUnit, NSubstitute, FluentAssertions (backend)
- 188 automated tests (113 backend across 7 xUnit projects + 75 frontend via Vitest)

## Project Structure

```
src/
├── BloomWatch.Api/                          # HTTP host -- entry point, middleware, endpoint registration
├── BloomWatch.SharedKernel/                 # Shared abstractions used across modules
├── BloomWatch.UI/                           # Angular 21 frontend application
│   └── src/
│       ├── app/
│       │   ├── core/layout/                 # Shell layout (nav bar) and minimal layout (auth pages)
│       │   ├── features/                    # Feature modules: landing, auth (register, login), dashboard, watch-spaces, settings, showcase
│       │   └── shared/
│       │       ├── styles/                  # Design tokens, base styles, animations, utilities
│       │       └── ui/                      # Bloom component library (button, card, input, badge, avatar)
│       └── styles.scss                      # Global stylesheet
└── Modules/
    ├── Identity/
    │   ├── BloomWatch.Modules.Identity.Domain/          # Aggregates, value objects, repository interfaces
    │   ├── BloomWatch.Modules.Identity.Application/     # Use case handlers, service abstractions
    │   ├── BloomWatch.Modules.Identity.Infrastructure/  # EF Core, BCrypt, JWT, migrations
    │   └── BloomWatch.Modules.Identity.Contracts/       # Integration events for inter-module communication
    ├── WatchSpaces/
    │   ├── BloomWatch.Modules.WatchSpaces.Domain/          # WatchSpace aggregate, Invitation entity, member rules
    │   ├── BloomWatch.Modules.WatchSpaces.Application/     # 12 use case handlers (create, invite, accept, leave, ...)
    │   ├── BloomWatch.Modules.WatchSpaces.Infrastructure/  # EF Core, email lookup, migrations
    │   └── BloomWatch.Modules.WatchSpaces.Contracts/       # Integration events for downstream modules
    ├── AniListSync/
    │   ├── BloomWatch.Modules.AniListSync.Domain/          # MediaCacheEntry entity, persistent caching
    │   ├── BloomWatch.Modules.AniListSync.Application/     # Search + media detail query handlers
    │   ├── BloomWatch.Modules.AniListSync.Infrastructure/  # AniList GraphQL client, EF Core + in-memory caching
    │   └── BloomWatch.Modules.AniListSync.Contracts/       # (reserved for integration events)
    └── AnimeTracking/
        ├── BloomWatch.Modules.AnimeTracking.Domain/          # WatchSpaceAnime aggregate, ParticipantEntry, WatchSession entities
        ├── BloomWatch.Modules.AnimeTracking.Application/     # Add, list, detail use cases; cross-module media cache lookup
        ├── BloomWatch.Modules.AnimeTracking.Infrastructure/  # EF Core, cross-module adapters
        └── BloomWatch.Modules.AnimeTracking.Contracts/       # (reserved for integration events)

tests/
├── BloomWatch.Modules.Identity.UnitTests/              # Domain + use case unit tests
├── BloomWatch.Modules.Identity.IntegrationTests/       # HTTP endpoint integration tests
├── BloomWatch.Modules.WatchSpaces.UnitTests/           # Domain unit tests
├── BloomWatch.Modules.WatchSpaces.IntegrationTests/    # HTTP endpoint integration tests
├── BloomWatch.Modules.AniListSync.UnitTests/           # Handler + caching unit tests
├── BloomWatch.Modules.AniListSync.IntegrationTests/    # HTTP endpoint integration tests
└── BloomWatch.Modules.AnimeTracking.UnitTests/         # Domain + use case unit tests

docs/
├── architecture.md     # Full system design, module breakdown, and planned feature phases
├── ui-ux-doctrine.md   # Authoritative UI/UX design doctrine (kawaii/Y2K system, tokens, component patterns)
├── user-stories.md     # Product user stories and completion tracking
└── wireframes/         # HTML/CSS wireframes for all frontend stories

openspec/               # Spec-driven change management (see below)
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) and npm 10+ (for the Angular frontend)
- PostgreSQL running locally (default: `localhost:5432`)

## Getting Started

### 1. Clone and restore dependencies

```bash
git clone <repo-url>
cd bloomwatch
dotnet restore
```

### 2. Configure the database connection

The default connection string in `appsettings.json` targets a local PostgreSQL instance:

```
Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres
```

Override it in `appsettings.Development.json` or via environment variable if your setup differs:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=bloomwatch;Username=youruser;Password=yourpass"
```

### 3. Apply migrations

```bash
./scripts/apply-migrations.sh
```

### 4. Run the API

```bash
dotnet run --project src/BloomWatch.Api
```

The API is available at `http://localhost:5192` (or `https://localhost:7073`).

In development mode, interactive API documentation is served by Scalar at `/scalar/v1`.

### 5. Run the frontend

```bash
cd src/BloomWatch.UI
npm install
npm start
```

The Angular dev server starts at `http://localhost:4200`.

## API Endpoints

### Interactive documentation

When running in Development mode, visit `/scalar/v1` for a full interactive API reference powered by Scalar and the OpenAPI spec.

### Identity

```http
POST /auth/register                                    # Create a new user account
POST /auth/login                                       # Authenticate and receive a JWT
GET  /users/me                                         # Get the authenticated user's profile
```

### WatchSpaces

All WatchSpace endpoints require a valid JWT (`Authorization: Bearer <token>`).

```http
POST   /watchspaces                                      # Create a new watch space
GET    /watchspaces                                      # List spaces you belong to
GET    /watchspaces/{id}                                 # Get a single space (members only)
PATCH  /watchspaces/{id}                                 # Rename a space (owner only)
POST   /watchspaces/{id}/transfer-ownership              # Transfer ownership to a member

POST   /watchspaces/{id}/invitations                     # Invite a user by email (owner only)
GET    /watchspaces/{id}/invitations                     # List pending invitations (owner only)
DELETE /watchspaces/{id}/invitations/{invitationId}      # Revoke an invitation (owner only)

POST   /watchspaces/invitations/{token}/accept           # Accept an invitation by token
POST   /watchspaces/invitations/{token}/decline          # Decline an invitation by token

DELETE /watchspaces/{id}/members/{userId}                # Remove a member (owner only)
DELETE /watchspaces/{id}/members/me                      # Leave a space
```

### AniList

Requires a valid JWT.

```http
GET /api/anilist/search?query=cowboy+bebop              # Search for anime via AniList (cached in-memory, 5 min)
GET /api/anilist/media/{anilistMediaId}                  # Get full media detail (cached in PostgreSQL)
```

### AnimeTracking

All AnimeTracking endpoints require a valid JWT and watch space membership.

```http
POST /watchspaces/{id}/anime                               # Add an anime by AniList media ID
GET  /watchspaces/{id}/anime                               # List anime in a watch space (optional ?status= filter)
GET  /watchspaces/{id}/anime/{watchSpaceAnimeId}            # Get full anime detail with participants and sessions
```

A `.http` file (`src/BloomWatch.Api/BloomWatch.Api.http`) is included for quick manual testing in VS Code or Rider.

## Frontend

The Angular frontend lives in `src/BloomWatch.UI/` and provides the user-facing application shell.

### Routing structure

| Route | Layout | Description |
|-------|--------|-------------|
| `/` | Minimal | Landing page (public, no nav bar) |
| `/login` | Minimal | Login page (no nav bar) |
| `/register` | Minimal | Registration page (no nav bar) |
| `/dashboard` | Shell | Dashboard |
| `/watch-spaces` | Shell | Watch space list and detail views |
| `/settings` | Shell | User settings |
| `/showcase` | Shell | Component library showcase |

### Bloom component library

The frontend includes a custom component library under `src/app/shared/ui/`, built with the kawaii/Y2K design system. All components use the `bloom-` selector prefix and follow Angular standalone component patterns with signal-based inputs and outputs.

| Component | Selector | Description |
|-----------|----------|-------------|
| Button | `bloom-button` | Primary, secondary, ghost, and danger variants with gel shine effects |
| Card | `bloom-card` | Content container with multiple visual variants |
| Input | `bloom-input` | Form input with label, validation states, and size options |
| Badge | `bloom-badge` | Status and category badges with color options |
| Avatar | `bloom-avatar` | User avatar with status indicators |
| Avatar Stack | `bloom-avatar-stack` | Overlapping avatar group display |
| Theme Toggle | `bloom-theme-toggle` | Light/dark theme switcher |

### Design system

The design system is defined through SCSS partials in `src/app/shared/styles/`:

- **`_tokens.scss`** -- CSS custom property design tokens (colors, typography, spacing, shadows, radii, animation timings)
- **`_base.scss`** -- Global resets and base element styles
- **`_animations.scss`** -- Keyframe animations (sparkle, float, bounce, shimmer, gel press effects)
- **`_utilities.scss`** -- Utility classes for common patterns

## Configuration

All configuration lives in `appsettings.json`. The key sections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<postgres connection string>"
  },
  "Identity": {
    "Jwt": {
      "SecretKey": "<long random secret -- change before deploying>",
      "Issuer": "BloomWatch",
      "Audience": "BloomWatch"
    }
  }
}
```

## Running Tests

```bash
# All backend tests
dotnet test

# By module
dotnet test tests/BloomWatch.Modules.Identity.UnitTests
dotnet test tests/BloomWatch.Modules.Identity.IntegrationTests
dotnet test tests/BloomWatch.Modules.WatchSpaces.UnitTests
dotnet test tests/BloomWatch.Modules.WatchSpaces.IntegrationTests
dotnet test tests/BloomWatch.Modules.AniListSync.UnitTests
dotnet test tests/BloomWatch.Modules.AniListSync.IntegrationTests
dotnet test tests/BloomWatch.Modules.AnimeTracking.UnitTests

# Frontend tests
cd src/BloomWatch.UI && npm test
```

Backend integration tests use an in-memory SQLite database via `WebApplicationFactory` -- no running database required.

## OpenSpec Workflow

BloomWatch uses [OpenSpec](openspec/config.yaml) -- a spec-driven change management workflow -- to plan, document, and track every meaningful feature change before and during implementation.

### How it works

Each change moves through three stages:

```
propose -> apply -> archive
```

| Stage | What happens |
|-------|-------------|
| **propose** | A change is described with a proposal (why/what/impact), a design doc, feature specs, and a task list |
| **apply** | Tasks are worked through one at a time, guided by the specs |
| **archive** | The completed change is moved to `openspec/changes/archive/` for historical reference |

### Directory layout

```
openspec/
├── config.yaml                   # OpenSpec configuration
├── specs/                        # Standalone feature specs (reusable across changes)
│   ├── user-registration/
│   ├── user-authentication/
│   ├── user-profile/
│   ├── auth-token-management/
│   ├── watch-space-management/
│   ├── watch-space-invitations/
│   ├── watch-space-membership/
│   ├── anilist-search/
│   ├── anilist-media-detail/
│   ├── add-anime-to-watch-space/
│   ├── list-watch-space-anime/
│   ├── watch-space-anime-detail/
│   ├── angular-routing-shell/
│   ├── http-client-setup/
│   ├── auth-interceptor/
│   ├── theme-switching/
│   ├── landing-page/
│   ├── registration-form/
│   └── login-form/
└── changes/
    └── archive/                  # Completed changes
```

### Completed changes

| Change | Date | Summary |
|--------|------|---------|
| `scaffold-identity-module` | 2026-03-13 | User registration, JWT login, full Identity module (domain to API) |
| `watchspaces-module` | 2026-03-13 | Watch space creation, email invitations, membership management |
| `user-profile-endpoint` | 2026-03-13 | `GET /users/me` authenticated profile retrieval |
| `anilist-search-proxy` | 2026-03-16 | `GET /api/anilist/search` with GraphQL proxy and in-memory caching |
| `angular-project-setup-routing-shell` | 2026-03-16 | Angular 21 frontend scaffold, routing shell, kawaii/Y2K design system, component library |
| `anilist-media-detail-backend` | 2026-03-16 | `GET /api/anilist/media/{id}` with persistent PostgreSQL caching |
| `http-client-and-auth-interceptor` | 2026-03-16 | Angular HTTP client, API service, JWT auth interceptor |
| `theme-system-light-dark-mode` | 2026-03-17 | Signal-based ThemeService with light/dark toggle and persistence |
| `add-anime-to-watch-space` | 2026-03-17 | AnimeTracking module: add anime to watch space with media cache integration |
| `landing-page` | 2026-03-17 | Landing page with hero section, feature cards, and CTAs |
| `list-anime-in-watch-space-backend` | 2026-03-17 | `GET /watchspaces/{id}/anime` with status filter and participant summaries |
| `registration-page-frontend` | 2026-03-17 | Registration form with validation, auto-login, and error handling |
| `get-anime-detail-backend` | 2026-03-18 | `GET /watchspaces/{id}/anime/{animeId}` with full aggregate detail |
| `login-page` | 2026-03-18 | Login form with validation and error handling |

### Working with OpenSpec

The workflow is integrated with Claude Code via slash commands:

```
/opsx:propose   -- describe a new feature and generate proposal + specs + tasks
/opsx:explore   -- think through requirements or investigate an existing change
/opsx:apply     -- implement tasks from an active change
/opsx:archive   -- finalize and archive a completed change
```

## Architecture

See [`docs/architecture.md`](docs/architecture.md) for the full system design, including:

- Modular monolith structure and module boundaries
- DDD layer responsibilities (Domain / Application / Infrastructure / Contracts)
- Database schema strategy (one PostgreSQL schema per module)
- Planned feature phases (joint backlog, AniList integration, compatibility analytics)
