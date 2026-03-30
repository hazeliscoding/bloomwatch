---
name: BloomWatch tech stack and architecture
description: Core stack and architectural conventions for the BloomWatch project
type: project
---

BloomWatch is a shared anime tracker platform for pairs/small groups.

**Why:** Useful for all future implementation tasks to understand conventions upfront.

**How to apply:** Follow these conventions when implementing new modules or features.

## Backend

- .NET 10, ASP.NET Core Minimal APIs
- EF Core 9 + Npgsql (PostgreSQL); one `DbContext` + schema per module
- Architecture: DDD modular monolith — 5 modules: Identity, WatchSpaces, AniListSync, AnimeTracking, Analytics
- Each module: `Domain / Application / Infrastructure / Contracts` layers
- Module registration: `Add<Name>Module(IServiceCollection, IConfiguration)` in `Infrastructure/Extensions/`
- API endpoints: static `Map<Name>Endpoints()` in `src/BloomWatch.Api/Modules/<Name>/`
- Cross-module reads: read-only `DbContext` in `Infrastructure/CrossModule/` — never call another module's services
- Auth: JWT HS256, 1-hour expiry; config key `Identity:Jwt:SecretKey`
- Solution file: `BloomWatch.slnx` (not `.sln`)
- Tests: xUnit + NSubstitute + FluentAssertions; integration tests use SQLite in-memory via `WebApplicationFactory<Program>`

## Frontend

- Angular 21, standalone components (no NgModules)
- Signals-based state throughout (`signal()`, `computed()`)
- `ApiService` (`core/http/api.service.ts`) — single HTTP abstraction; all feature services use it
- `authInterceptor` attaches `Authorization: Bearer` to all requests
- Design system: kawaii/Y2K, `bloom-` component prefix, SCSS tokens in `shared/styles/_tokens.scss`
- Dev env points to `http://localhost:5192`; prod uses `/api`
- `showcase` route is dev-only (excluded from prod build via `environment.production`)

## Status

MVP complete as of 2026-03-30. All 47 stories / 153 pts shipped. Open issues are post-MVP backlog tracked in GitHub.
