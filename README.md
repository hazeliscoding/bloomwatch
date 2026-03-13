# BloomWatch

A shared anime tracking platform for pairs and small groups. BloomWatch lets friends maintain a joint backlog, log watch history, leave separate ratings, and surface compatibility analytics — without relying on spreadsheets or Discord threads.

## Tech Stack

- **Runtime:** .NET 10 / ASP.NET Core (Minimal APIs)
- **Database:** PostgreSQL — one schema per module
- **ORM:** Entity Framework Core 9 + Npgsql
- **Auth:** JWT HS256 (1-hour expiry) + BCrypt password hashing (work factor 12)
- **Architecture:** DDD-inspired modular monolith
- **Testing:** xUnit, NSubstitute, FluentAssertions

## Project Structure

```
src/
├── BloomWatch.Api/                          # HTTP host — entry point, middleware, endpoint registration
├── BloomWatch.SharedKernel/                 # Shared abstractions used across modules
└── Modules/
    └── Identity/
        ├── BloomWatch.Modules.Identity.Domain/          # Aggregates, value objects, repository interfaces
        ├── BloomWatch.Modules.Identity.Application/     # Use case command handlers, service abstractions
        ├── BloomWatch.Modules.Identity.Infrastructure/  # EF Core, BCrypt, JWT, migrations
        └── BloomWatch.Modules.Identity.Contracts/       # Integration events for inter-module communication

tests/
├── BloomWatch.Modules.Identity.UnitTests/          # Domain + use case unit tests (25 tests)
└── BloomWatch.Modules.Identity.IntegrationTests/   # HTTP endpoint integration tests (6 tests)

docs/
└── architecture.md     # Full system design, module breakdown, and planned feature phases

openspec/               # Spec-driven change management (see below)
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL running locally (default: `localhost:5432`)

## Getting Started

**1. Clone and restore dependencies**

```bash
git clone <repo-url>
cd bloomwatch
dotnet restore
```

**2. Configure the database connection**

The default connection string in `appsettings.json` targets a local PostgreSQL instance:

```
Host=localhost;Database=bloomwatch;Username=postgres;Password=postgres
```

Override it in `appsettings.Development.json` or via environment variable if your setup differs:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=bloomwatch;Username=youruser;Password=yourpass"
```

**3. Apply migrations**

```bash
dotnet ef database update --project src/Modules/Identity/BloomWatch.Modules.Identity.Infrastructure \
                          --startup-project src/BloomWatch.Api
```

**4. Run the API**

```bash
dotnet run --project src/BloomWatch.Api
```

The API will be available at `http://localhost:5192` (or `https://localhost:7073`).

## API Endpoints

### Register a user

```http
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "yourpassword",
  "displayName": "YourName"
}
```

### Log in

```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "yourpassword"
}
```

Returns a signed JWT access token valid for 1 hour.

A `.http` file (`src/BloomWatch.Api/BloomWatch.Api.http`) is included for quick manual testing in VS Code or Rider.

## Configuration

All configuration lives in `appsettings.json`. The key sections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<postgres connection string>"
  },
  "Identity": {
    "Jwt": {
      "SecretKey": "<long random secret — change before deploying>",
      "Issuer": "BloomWatch",
      "Audience": "BloomWatch"
    }
  }
}
```

## Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/BloomWatch.Modules.Identity.UnitTests

# Integration tests only
dotnet test tests/BloomWatch.Modules.Identity.IntegrationTests
```

Integration tests use an in-memory SQLite database via `WebApplicationFactory` — no real database required.

## OpenSpec Workflow

BloomWatch uses [OpenSpec](openspec/config.yaml) — a spec-driven change management workflow — to plan, document, and track every meaningful feature change before and during implementation.

### How it works

Each change moves through three stages:

```
propose → apply → archive
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
│   └── user-authentication/
└── changes/
    └── archive/
        └── 2026-03-13-scaffold-identity-module/   # Completed Identity module change
            ├── proposal.md       # Why this change was made and what it includes
            ├── design.md         # Technical design decisions
            ├── tasks.md          # All 39 tasks, fully checked off
            └── specs/            # Specs used during this change
```

### Completed changes

| Change | Date | Summary |
|--------|------|---------|
| `scaffold-identity-module` | 2026-03-13 | User registration, JWT login, full Identity module (domain → API) |

### Working with OpenSpec

The workflow is integrated with Claude Code via slash commands:

```
/opsx:propose   — describe a new feature and generate proposal + specs + tasks
/opsx:explore   — think through requirements or investigate an existing change
/opsx:apply     — implement tasks from an active change
/opsx:archive   — finalize and archive a completed change
```

## Architecture

See [`docs/architecture.md`](docs/architecture.md) for the full system design, including:

- Modular monolith structure and module boundaries
- DDD layer responsibilities (Domain / Application / Infrastructure / Contracts)
- Database schema strategy (one PostgreSQL schema per module)
- Planned feature phases (joint backlog, AniList integration, compatibility analytics)
