# 🌸 BloomWatch

> A shared anime tracking platform for pairs and small groups.

BloomWatch lets friends maintain a joint backlog, track watch progress, leave separate ratings, and discover how compatible their taste really is — no more spreadsheets or Discord threads.

## ✨ Features

- **Watch Spaces** — Create shared spaces for you and your watch partner(s)
- **AniList Integration** — Search and add anime directly from AniList, with cached metadata
- **Individual Progress** — Everyone tracks their own episodes and status independently
- **Ratings & Reviews** — Personal 0.5–10 ratings with optional notes
- **Compatibility Analytics** — See how your ratings align, find your biggest disagreements, and get random backlog picks
- **Kawaii/Y2K Design** — A playful pastel design system with gel effects, light/dark themes, and custom components

## 🛠 Tech Stack

| Layer | Tech |
|-------|------|
| **Backend** | .NET 10 · ASP.NET Core Minimal APIs · PostgreSQL · EF Core 9 · JWT auth |
| **Frontend** | Angular 21 · SCSS design tokens · Vitest |
| **Architecture** | DDD-inspired modular monolith (5 modules, one DB schema each) |
| **Testing** | 700+ automated tests (180 xUnit backend + 527 Vitest frontend) |
| **API Docs** | OpenAPI + [Scalar](https://scalar.com/) interactive explorer at `/scalar/v1` |

## 🚀 Quick Start

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) · [Node.js 18+](https://nodejs.org/) · PostgreSQL · [Docker](https://www.docker.com/) (optional, for local email)

```bash
# 1. Clone and restore
git clone <repo-url> && cd bloomwatch
dotnet restore

# 2. Configure the database (override if your setup differs)
#    Default: Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres
export ConnectionStrings__DefaultConnection="Host=localhost;Database=bloomwatch;Username=youruser;Password=yourpass"

# 3. (Optional) Start Mailpit to capture invitation emails locally
docker run -d -p 1025:1025 -p 8025:8025 axllent/mailpit
# → SMTP on localhost:1025  ·  Web UI at http://localhost:8025

# 4. Apply migrations and start the API
./scripts/apply-migrations.sh
dotnet run --project src/BloomWatch.Api
# → http://localhost:5192  (API docs at /scalar/v1)

# 5. Start the frontend
cd src/BloomWatch.UI && npm install && npm start
# → http://localhost:4200
```

## 🧪 Running Tests

```bash
dotnet test                          # All backend tests
cd src/BloomWatch.UI && npm test     # Frontend tests
```

Backend integration tests use in-memory SQLite via `WebApplicationFactory` — no running database needed.

## � Interactive API Docs (Scalar)

With the API running (`dotnet run --project src/BloomWatch.Api`), open **[http://localhost:5192/scalar/v1](http://localhost:5192/scalar/v1)** in your browser to access the Scalar API explorer.

Scalar provides:

- **Browse all 30 endpoints** grouped by module (Identity, WatchSpaces, AniList, AnimeTracking, Analytics, Home)
- **Try requests live** — fill in parameters, set headers, and execute calls against your local API
- **Authenticate** — click the 🔒 lock icon, paste a JWT obtained from `POST /auth/login`, and all subsequent requests include the `Authorization: Bearer` header automatically
- **View request/response schemas** — expand any endpoint to see the full JSON shape, required fields, and status codes

> **Tip:** Register a user via `POST /auth/register`, then log in via `POST /auth/login` to get a token. Paste it into Scalar's auth dialog and you can explore every protected endpoint interactively.

The raw OpenAPI JSON spec is also available at [`/openapi/v1.json`](http://localhost:5192/openapi/v1.json) for importing into other tools (Postman, Insomnia, etc.).

## 📁 Project Structure

```
src/
├── BloomWatch.Api/              # HTTP host, endpoint registration, middleware
├── BloomWatch.SharedKernel/     # Cross-cutting abstractions
├── BloomWatch.UI/               # Angular frontend (features, shared UI, design system)
└── Modules/
    ├── Identity/                # Registration, login, JWT, user profiles
    ├── WatchSpaces/             # Watch space CRUD, invitations, membership
    ├── AniListSync/             # AniList GraphQL proxy with persistent caching
    ├── AnimeTracking/           # Anime lifecycle, progress, ratings per watch space
    └── Analytics/               # Compatibility scores, rating gaps, shared stats

tests/                           # Unit + integration tests per module (10 xUnit projects)
docs/                            # Architecture, UI/UX doctrine, user stories, wireframes
openspec/                        # Spec-driven change tracking (see below)
```

Each module follows a consistent DDD layering: **Domain → Application → Infrastructure → Contracts**.

## 🧩 Modules

| Module | Schema | Responsibility |
|--------|--------|---------------|
| **Identity** | `identity` | User registration, login, JWT authentication, and profile management |
| **WatchSpaces** | `watch_spaces` | Shared space lifecycle, invitations, membership, and ownership transfer |
| **AniListSync** | `anilist_sync` | AniList GraphQL proxy with persistent `media_cache` table (24-hour freshness) and in-memory search cache (5-minute TTL) |
| **AnimeTracking** | `anime_tracking` | Per-space anime lifecycle, shared group status, individual participant progress/ratings |
| **Analytics** | *(read-only)* | Compatibility scores, rating gap analysis, shared stats, dashboard summaries, and random backlog picks |

Modules communicate through **read-only cross-module `DbContext`s** — never by calling another module's services or repositories directly. This preserves bounded context isolation while sharing a single database.

## 🎨 Design System

The frontend ships a custom `bloom-*` component library with a kawaii/Y2K aesthetic (Quicksand + Nunito typefaces, pastel palette, gel effects). Components include buttons, cards, inputs, badges, avatars, modals, a compatibility ring, and a backlog picker.

Design tokens, animations, and utilities live in `src/BloomWatch.UI/src/app/shared/styles/`. See [`docs/ui-ux-doctrine.md`](docs/ui-ux-doctrine.md) for the full design system reference.

## 📐 Architecture

This is a **modular monolith** — five domain modules sharing a single deployment but separated by schema, bounded context, and clean cross-module contracts.

See [`docs/architecture.md`](docs/architecture.md) for the full design doc covering module boundaries, database strategy, and planned feature phases.

## 📝 OpenSpec Workflow

BloomWatch uses a spec-driven change management workflow. Every feature is planned as a proposal with a design doc, feature specs, and a task checklist before implementation begins.

```
propose → apply → archive
```

Completed changes are archived in `openspec/changes/archive/`. Feature specs live in `openspec/specs/`.

## 📜 License

MIT
