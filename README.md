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
| **Testing** | 446 automated tests (xUnit + Vitest) |
| **API Docs** | OpenAPI + [Scalar](https://scalar.com/) interactive explorer at `/scalar/v1` |

## 🚀 Quick Start

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) · [Node.js 18+](https://nodejs.org/) · PostgreSQL

```bash
# 1. Clone and restore
git clone <repo-url> && cd bloomwatch
dotnet restore

# 2. Configure the database (override if your setup differs)
#    Default: Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres
export ConnectionStrings__DefaultConnection="Host=localhost;Database=bloomwatch;Username=youruser;Password=yourpass"

# 3. Apply migrations and start the API
./scripts/apply-migrations.sh
dotnet run --project src/BloomWatch.Api
# → http://localhost:5192  (API docs at /scalar/v1)

# 4. Start the frontend
cd src/BloomWatch.UI && npm install && npm start
# → http://localhost:4200
```

## 🧪 Running Tests

```bash
dotnet test                          # All backend tests
cd src/BloomWatch.UI && npm test     # Frontend tests
```

Backend integration tests use in-memory SQLite via `WebApplicationFactory` — no running database needed.

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
