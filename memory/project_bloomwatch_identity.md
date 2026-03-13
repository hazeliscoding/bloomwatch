---
name: BloomWatch Identity Module - Scaffold Complete
description: Identity module (User aggregate, registration, JWT auth) was scaffolded and implemented
type: project
---

The Identity module scaffold is complete and all 39 tasks were implemented.

**Why:** Foundation module required before any other module — all other modules depend on user identity.

**How to apply:** When working on any feature that needs authentication, the Identity module is already in place. The JWT middleware is wired into `Program.cs`. New protected endpoints just need `[Authorize]`.

Key facts:
- Solution file: `BloomWatch.slnx` (not .sln — .NET 10 uses .slnx format)
- Identity module lives at `src/Modules/Identity/BloomWatch.Modules.Identity.{Domain,Application,Infrastructure,Contracts}/`
- API project: `src/BloomWatch.Api/`
- PostgreSQL schema: `identity`, table: `users`
- JWT: HS256, 1-hour expiry, config key `Identity:Jwt:SecretKey`
- Password hashing: BCrypt.Net-Next, work factor 12
- EF Core migration exists: `InitialIdentitySchema` (run `dotnet ef database update` when DB is available)
- `IdentityDbContextFactory` in Infrastructure for design-time migrations
- Integration tests use SQLite in-memory (shared connection via `IdentityWebAppFactory`)
- Unit tests: 25 passing; Integration tests: 6 passing
- Two placeholder integration tests for JWT middleware (tasks 7.8) await a real `[Authorize]` endpoint
